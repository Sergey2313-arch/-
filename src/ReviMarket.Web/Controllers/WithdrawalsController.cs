using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class WithdrawalsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public WithdrawalsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        ViewBag.Wallet = await GetOrCreateWalletAsync(uid);
        return View(await _db.WithdrawalRequests
            .Where(x => x.UserId == uid)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(decimal amount, string paymentInfo)
    {
        var uid = _users.GetUserId(User)!;
        paymentInfo = (paymentInfo ?? string.Empty).Trim();

        if (amount is < 100 or > 1_000_000 || paymentInfo.Length is < 3 or > 300)
        {
            return RedirectToAction(nameof(Index));
        }

        await GetOrCreateWalletAsync(uid);
        await using var tx = await _db.Database.BeginTransactionAsync();

        var reserved = await _db.Wallets
            .Where(x => x.UserId == uid && x.Balance >= amount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Balance, x => x.Balance - amount)
                .SetProperty(x => x.HoldBalance, x => x.HoldBalance + amount));

        if (reserved == 0)
        {
            await tx.RollbackAsync();
            return RedirectToAction(nameof(Index));
        }

        _db.WithdrawalRequests.Add(new WithdrawalRequest
        {
            UserId = uid,
            Amount = amount,
            PaymentInfo = paymentInfo
        });
        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = uid,
            Amount = amount,
            Type = PaymentTypes.Withdraw,
            Status = PaymentStatuses.Pending,
            Comment = "Заявка на вывод средств"
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
