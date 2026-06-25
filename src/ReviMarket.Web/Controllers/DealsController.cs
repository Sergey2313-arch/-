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
    private const decimal Fee = 10m;
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

        return View(deals);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string executorEmail, decimal amount, int? marketItemId)
    {
        var customerId = _users.GetUserId(User)!;
        var executor = await _users.FindByEmailAsync(executorEmail.Trim());
        if (executor is null || executor.Id == customerId || amount <= 0) return View();

        var wallet = await GetWallet(customerId);
        if (wallet.Balance < amount) return View();

        var feeAmount = Math.Round(amount * Fee / 100m, 2);

        await using var tx = await _db.Database.BeginTransactionAsync();

        wallet.Balance -= amount;
        wallet.HoldBalance += amount;

        _db.Deals.Add(new Deal
        {
            CustomerId = customerId,
            ExecutorId = executor.Id,
            MarketItemId = marketItemId,
            Amount = amount,
            CommissionPercent = Fee,
            CommissionAmount = feeAmount,
            ExecutorAmount = amount - feeAmount,
            Status = DealStatuses.Funded
        });

        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = customerId,
            Amount = amount,
            Type = PaymentTypes.Hold,
            Status = PaymentStatuses.Success
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var uid = _users.GetUserId(User)!;
        var deal = await _db.Deals.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == uid);
        if (deal is null || deal.Status != DealStatuses.Funded) return NotFound();

        var customerWallet = await GetWallet(deal.CustomerId);
        var executorWallet = await GetWallet(deal.ExecutorId);

        await using var tx = await _db.Database.BeginTransactionAsync();

        customerWallet.HoldBalance -= deal.Amount;
        executorWallet.Balance += deal.ExecutorAmount;
        deal.Status = DealStatuses.Completed;
        deal.CompletedAt = DateTime.UtcNow;

        _db.PaymentTransactions.Add(new PaymentTransaction { UserId = deal.ExecutorId, Amount = deal.ExecutorAmount, Type = PaymentTypes.Release, Status = PaymentStatuses.Success });
        _db.PaymentTransactions.Add(new PaymentTransaction { UserId = deal.CustomerId, Amount = deal.CommissionAmount, Type = PaymentTypes.Commission, Status = PaymentStatuses.Success });
        _db.PlatformTransactions.Add(new PlatformTransaction { Amount = deal.CommissionAmount, Type = PlatformTransactionTypes.Commission, DealId = deal.Id });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction(nameof(Index));
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
