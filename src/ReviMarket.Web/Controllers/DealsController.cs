using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class DealsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DealsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        var deals = await _db.Deals
            .Include(x => x.Customer)
            .Include(x => x.Executor)
            .Include(x => x.MarketItem)
            .Where(x => x.CustomerId == uid || x.ExecutorId == uid)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        ViewBag.MyReviews = await _db.UserReviews.Where(x => x.AuthorId == uid).ToListAsync();
        return View(deals);
    }

    [HttpGet]
    public IActionResult Create() => RedirectToAction("Index", "Orders");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(string executorEmail, decimal amount, int? marketItemId) => RedirectToAction("Index", "Orders");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var uid = _users.GetUserId(User)!;
        var deal = await _db.Deals
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == uid);

        if (deal is null) return NotFound();

        await GetOrCreateWalletAsync(deal.CustomerId);
        await GetOrCreateWalletAsync(deal.ExecutorId);

        await using var tx = await _db.Database.BeginTransactionAsync();
        var completedAt = DateTime.UtcNow;

        var claimed = await _db.Deals
            .Where(x => x.Id == id && x.CustomerId == uid && x.Status == DealStatuses.Funded)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, DealStatuses.Completed)
                .SetProperty(x => x.CompletedAt, completedAt));

        if (claimed == 0)
        {
            await tx.RollbackAsync();
            return Conflict("Сделка уже завершена или недоступна");
        }

        var holdReleased = await _db.Wallets
            .Where(x => x.UserId == deal.CustomerId && x.HoldBalance >= deal.Amount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.HoldBalance, x => x.HoldBalance - deal.Amount));

        if (holdReleased == 0)
        {
            await tx.RollbackAsync();
            return Conflict("Недостаточно средств в резерве сделки");
        }

        await _db.Wallets
            .Where(x => x.UserId == deal.ExecutorId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Balance, x => x.Balance + deal.ExecutorAmount));

        if (deal.MarketItemId is not null)
        {
            await _db.MarketItems
                .Where(x => x.Id == deal.MarketItemId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.OrderStatus, OrderStatuses.Done));
        }

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = deal.ExecutorId,
            Amount = deal.ExecutorAmount,
            Type = PaymentTypes.Release,
            Status = PaymentStatuses.Success,
            Comment = $"Выплата по сделке #{deal.Id}"
        });
        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = deal.CustomerId,
            Amount = deal.CommissionAmount,
            Type = PaymentTypes.Commission,
            Status = PaymentStatuses.Success,
            Comment = $"Комиссия по сделке #{deal.Id}"
        });
        _db.PlatformTransactions.Add(new PlatformTransaction
        {
            Amount = deal.CommissionAmount,
            Type = PlatformTransactionTypes.Commission,
            DealId = deal.Id
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction(nameof(Index));
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
