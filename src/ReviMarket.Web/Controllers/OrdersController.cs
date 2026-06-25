using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

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
        var query = _db.MarketItems.Include(x => x.Owner).Include(x => x.AssignedExecutor).Where(x => x.Type == MarketItemTypes.Order && x.ReviewStatus == ReviewStatuses.Approved);
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
        var item = await _db.MarketItems.Include(x => x.Owner).Include(x => x.AssignedExecutor).FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (item is null) return NotFound();
        var canSee = item.ReviewStatus == ReviewStatuses.Approved || item.OwnerId == uid || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Moderator);
        if (!canSee) return NotFound();
        ViewBag.Deal = await _db.Deals.FirstOrDefaultAsync(x => x.MarketItemId == id);
        return View(item);
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = MarketCategories.All;
        return View(new MarketItem { Type = MarketItemTypes.Order, Category = MarketCategories.Design });
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarketItem item)
    {
        if (item.Price < 100) ModelState.AddModelError(nameof(item.Price), "Минимальный бюджет — 100 ₽");
        if (string.IsNullOrWhiteSpace(item.Title)) ModelState.AddModelError(nameof(item.Title), "Укажи название заказа");
        if (string.IsNullOrWhiteSpace(item.Description)) ModelState.AddModelError(nameof(item.Description), "Опиши задачу");

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = MarketCategories.All;
            return View(item);
        }

        item.Type = MarketItemTypes.Order;
        item.OwnerId = _userManager.GetUserId(User);
        item.CreatedAt = DateTime.UtcNow;
        item.OrderStatus = OrderStatuses.Open;
        item.ReviewStatus = User.IsInRole(UserRoles.Admin) ? ReviewStatuses.Approved : ReviewStatuses.Pending;
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
        var order = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (order is null) return NotFound();
        if (order.ReviewStatus != ReviewStatuses.Approved || order.OwnerId == uid || order.OrderStatus != OrderStatuses.Open) return BadRequest();

        order.AssignedExecutorId = uid;
        order.AssignedAt = DateTime.UtcNow;
        order.OrderStatus = OrderStatuses.InWork;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = UserRoles.Customer + "," + UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fund(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        var order = await _db.MarketItems.FirstOrDefaultAsync(x => x.Id == id && x.Type == MarketItemTypes.Order);
        if (order is null) return NotFound();
        if (order.Price < 100 || order.ReviewStatus != ReviewStatuses.Approved || order.OwnerId != uid || string.IsNullOrWhiteSpace(order.AssignedExecutorId)) return BadRequest();
        if (await _db.Deals.AnyAsync(x => x.MarketItemId == id)) return RedirectToAction(nameof(Details), new { id });

        var wallet = await GetWallet(uid);
        if (wallet.Balance < order.Price) return RedirectToAction("TopUp", "Wallet");

        var fee = Math.Round(order.Price * Fee / 100m, 2);
        await using var tx = await _db.Database.BeginTransactionAsync();
        wallet.Balance -= order.Price;
        wallet.HoldBalance += order.Price;
        _db.Deals.Add(new Deal { CustomerId = uid, ExecutorId = order.AssignedExecutorId, MarketItemId = id, Amount = order.Price, CommissionPercent = Fee, CommissionAmount = fee, ExecutorAmount = order.Price - fee, Status = DealStatuses.Funded });
        _db.PaymentTransactions.Add(new PaymentTransaction { UserId = uid, Amount = order.Price, Type = PaymentTypes.Hold, Status = PaymentStatuses.Success });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Wallet> GetWallet(string userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet is not null) return wallet;
        wallet = new Wallet { UserId = userId };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }
}
