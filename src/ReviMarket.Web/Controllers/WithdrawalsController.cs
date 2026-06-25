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
        ViewBag.Wallet = await GetWallet(uid);
        return View(await _db.WithdrawalRequests.Where(x => x.UserId == uid).OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(decimal amount, string paymentInfo)
    {
        var uid = _users.GetUserId(User)!;
        var wallet = await GetWallet(uid);
        if (amount <= 0 || wallet.Balance < amount) return RedirectToAction(nameof(Index));

        wallet.Balance -= amount;
        wallet.HoldBalance += amount;

        _db.WithdrawalRequests.Add(new WithdrawalRequest { UserId = uid, Amount = amount, PaymentInfo = paymentInfo });
        _db.PaymentTransactions.Add(new PaymentTransaction { UserId = uid, Amount = amount, Type = PaymentTypes.Withdraw, Status = PaymentStatuses.Pending });

        await _db.SaveChangesAsync();
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
