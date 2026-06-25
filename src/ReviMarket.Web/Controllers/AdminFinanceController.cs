using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager + "," + UserRoles.Owner + "," + UserRoles.CoOwner)]
public class AdminFinanceController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminFinanceController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var deals = await _db.Deals.Include(x => x.Customer).Include(x => x.Executor).OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync();
        var withdrawals = await _db.WithdrawalRequests.Include(x => x.User).OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync();
        var wallets = await _db.Wallets.ToListAsync();
        var platformTransactions = await _db.PlatformTransactions.ToListAsync();

        ViewBag.TotalDeals = deals.Sum(x => x.Amount);
        ViewBag.TotalCommission = platformTransactions.Sum(x => x.Amount);
        ViewBag.TotalHold = wallets.Sum(x => x.HoldBalance);
        ViewBag.TotalBalance = wallets.Sum(x => x.Balance);
        ViewBag.Withdrawals = withdrawals;
        ViewBag.Deals = deals;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkWithdrawalPaid(int id)
    {
        var item = await _db.WithdrawalRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null || item.Status != WithdrawalStatuses.Pending) return RedirectToAction(nameof(Index));
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == item.UserId);
        if (wallet is not null && wallet.HoldBalance >= item.Amount) wallet.HoldBalance -= item.Amount;
        item.Status = WithdrawalStatuses.Paid;
        item.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectWithdrawal(int id)
    {
        var item = await _db.WithdrawalRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null || item.Status != WithdrawalStatuses.Pending) return RedirectToAction(nameof(Index));
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == item.UserId);
        if (wallet is not null)
        {
            wallet.HoldBalance = Math.Max(0, wallet.HoldBalance - item.Amount);
            wallet.Balance += item.Amount;
        }
        item.Status = WithdrawalStatuses.Rejected;
        item.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
