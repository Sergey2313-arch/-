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
        var deals = await _db.Deals.Include(x => x.Customer).Include(x => x.Executor).Include(x => x.MarketItem).Where(x => x.CustomerId == uid || x.ExecutorId == uid).OrderByDescending(x => x.CreatedAt).ToListAsync();
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
        var deal = await _db.Deals.Include(x => x.MarketItem).FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == uid);
        if (deal is null || deal.Status != DealStatuses.Funded) return NotFound();

        var customerWallet = await GetWallet(deal.CustomerId);
        var executorWallet = await GetWallet(deal.ExecutorId);

        await using var tx = await _db.Database.BeginTransactionAsync();
        customerWallet.HoldBalance -= deal.Amount;
        executorWallet.Balance += deal.ExecutorAmount;
        deal.Status = DealStatuses.Completed;
        deal.CompletedAt = DateTime.UtcNow;
        if (deal.MarketItem is not null) deal.MarketItem.OrderStatus = OrderStatuses.Done;

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
