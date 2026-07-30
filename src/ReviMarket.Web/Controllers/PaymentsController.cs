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
    private readonly IWebHostEnvironment _environment;

    public PaymentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        IWebHostEnvironment environment)
    {
        _db = db;
        _users = users;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        if (!_environment.IsDevelopment()) return StatusCode(StatusCodes.Status503ServiceUnavailable);

        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid && x.Provider == PaymentProviders.Test);

        if (invoice is null) return NotFound();
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDemo(int id)
    {
        if (!_environment.IsDevelopment()) return NotFound();

        var uid = _users.GetUserId(User)!;
        await GetOrCreateWalletAsync(uid);

        await using var tx = await _db.Database.BeginTransactionAsync();
        var paidAt = DateTime.UtcNow;

        var claimed = await _db.PaymentInvoices
            .Where(x => x.Id == id
                && x.UserId == uid
                && x.Provider == PaymentProviders.Test
                && x.Status == PaymentStatuses.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, PaymentStatuses.Success)
                .SetProperty(x => x.PaidAt, paidAt));

        if (claimed == 0)
        {
            await tx.RollbackAsync();
            return RedirectToAction("Index", "Wallet");
        }

        var invoice = await _db.PaymentInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == id && x.UserId == uid);

        await _db.Wallets
            .Where(x => x.UserId == uid)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Balance, x => x.Balance + invoice.Amount));

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
