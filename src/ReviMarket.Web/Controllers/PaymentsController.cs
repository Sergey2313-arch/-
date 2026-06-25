using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public PaymentsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
        if (invoice is null) return NotFound();
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDemo(int id)
    {
        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
        if (invoice is null) return NotFound();
        if (invoice.Status == PaymentStatuses.Success) return RedirectToAction("Index", "Wallet");

        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == uid);
        if (wallet is null)
        {
            wallet = new Wallet { UserId = uid };
            _db.Wallets.Add(wallet);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        invoice.Status = PaymentStatuses.Success;
        invoice.PaidAt = DateTime.UtcNow;
        wallet.Balance += invoice.Amount;
        _db.PaymentTransactions.Add(new PaymentTransaction
        {
            UserId = uid,
            Amount = invoice.Amount,
            Type = PaymentTypes.TopUp,
            Status = PaymentStatuses.Success,
            Comment = $"Демо-оплата счёта #{invoice.Id}"
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction("Index", "Wallet");
    }
}
