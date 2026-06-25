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
        ViewBag.TotalDeals = await _db.Deals.SumAsync(x => x.Amount);
        ViewBag.TotalCommission = await _db.PlatformTransactions.SumAsync(x => x.Amount);
        ViewBag.TotalHold = await _db.Wallets.SumAsync(x => x.HoldBalance);
        ViewBag.TotalBalance = await _db.Wallets.SumAsync(x => x.Balance);
        ViewBag.Withdrawals = await _db.WithdrawalRequests.Include(x => x.User).OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync();
        ViewBag.Deals = await _db.Deals.Include(x => x.Customer).Include(x => x.Executor).OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync();
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
            wallet.HoldBalance -= item.Amount;
            wallet.Balance += item.Amount;
        }
        item.Status = WithdrawalStatuses.Rejected;
        item.ProcessedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
