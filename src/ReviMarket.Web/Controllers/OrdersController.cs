using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.ViewModels;

namespace ReviMarket.Web.Controllers;

public class OrdersController : Controller
{
    private const decimal Fee = 10m;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _db.MarketItems
            .Include(x => x.Owner)
            .Include(x => x.AssignedExecutor)
            .Where(x => x.Type == MarketItemTypes.Order && x.ReviewStatus == ReviewStatuses.Approved);

        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));

        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Categories = MarketCategories.All;
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var uid = _userManager.GetUserId(User);
        var item = await _db.MarketItems
            .Include(x => x.Owner)
            .Include(x => x.AssignedExecutor)
            .FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);

        if (item is null) return NotFound();

        var canSee = item.ReviewStatus == ReviewStatuses.Approved
            || item.OwnerId == uid
            || User.IsInRole(UserRoles.Admin)
            || User.IsInRole(UserRoles.Moderator);

        if (!canSee) return NotFound();

        ViewBag.Deal = await _db.Deals.FirstOrDefaultAsync(x => x.MarketItemId == id);
        return View(item);
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = MarketCategories.All;
        return View(new CreateOrderViewModel());
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        if (!MarketCategories.All.Contains(model.Category, StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(model.Category), "Выбери категорию из списка");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = MarketCategories.All;
            return View(model);
        }

        var item = new MarketItem
        {
            Type = MarketItemTypes.Order,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Price = model.Price,
            Category = model.Category,
            OwnerId = _userManager.GetUserId(User),
            CreatedAt = DateTime.UtcNow,
            OrderStatus = OrderStatuses.Open,
            ReviewStatus = User.IsInRole(UserRoles.Admin) ? ReviewStatuses.Approved : ReviewStatuses.Pending,
            AssignedExecutorId = null,
            AssignedAt = null,
            ImagePath = null
        };

        _db.MarketItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Profile");
    }

    [Authorize(Roles = UserRoles.Creator + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        var assignedAt = DateTime.UtcNow;

        var affected = await _db.MarketItems
            .Where(x => x.Id == id
                && x.Type == MarketItemTypes.Order
                && x.ReviewStatus == ReviewStatuses.Approved
                && x.OwnerId != uid
                && x.OrderStatus == OrderStatuses.Open
                && x.AssignedExecutorId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.AssignedExecutorId, uid)
                .SetProperty(x => x.AssignedAt, assignedAt)
                .SetProperty(x => x.OrderStatus, OrderStatuses.InWork));

        if (affected == 0) return Conflict("Заказ уже занят или недоступен");
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fund(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        var order = await _db.MarketItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && x.Type == MarketItemTypes.Order
                && x.ReviewStatus == ReviewStatuses.Approved
                && x.OwnerId == uid
                && x.OrderStatus == OrderStatuses.InWork
                && x.AssignedExecutorId != null);

        if (order is null || order.Price < 100) return BadRequest();

        await GetOrCreateWalletAsync(uid);
        var fee = Math.Round(order.Price * Fee / 100m, 2);

        await using var tx = await _db.Database.BeginTransactionAsync();

        if (await _db.Deals.AnyAsync(x => x.MarketItemId == id))
        {
            await tx.RollbackAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        var walletUpdated = await _db.Wallets
            .Where(x => x.UserId == uid && x.Balance >= order.Price)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Balance, x => x.Balance - order.Price)
                .SetProperty(x => x.HoldBalance, x => x.HoldBalance + order.Price));

        if (walletUpdated == 0)
        {
            await tx.RollbackAsync();
            return RedirectToAction("TopUp", "Wallet");
        }

        _db.Deals.Add(new Deal
        {
            CustomerId = uid,
            ExecutorId = order.AssignedExecutorId!,
            MarketItemId = id,
            Amount = order.Price,
            CommissionPercent = Fee,
            CommissionAmount = fee,
            ExecutorAmount = order.Price - fee,
            Status = DealStatuses.Funded
        });

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = uid,
            Amount = order.Price,
            Type = PaymentTypes.Hold,
            Status = PaymentStatuses.Success,
            Comment = $"Резерв по заказу #{id}"
        });

        try
        {
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Wallet> GetOrCreateWalletAsync(string userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet is not null) return wallet;

        wallet = new Wallet { UserId = userId };
        _db.Wallets.Add(wallet);

        try
        {
            await _db.SaveChangesAsync();
            return wallet;
        }
        catch (DbUpdateException)
        {
            _db.Entry(wallet).State = EntityState.Detached;
            return await _db.Wallets.SingleAsync(x => x.UserId == userId);
        }
    }
}
