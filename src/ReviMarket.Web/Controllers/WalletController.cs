using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WalletController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var wallet = await GetOrCreateWalletAsync(userId);
        var transactions = await _db.PaymentTransactions.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(15).ToListAsync();
        var invoices = await _db.PaymentInvoices.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync();
        ViewBag.Transactions = transactions;
        ViewBag.Invoices = invoices;
        return View(wallet);
    }

    [HttpGet]
    public IActionResult TopUp() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopUp(decimal amount)
    {
        if (amount < 100)
        {
            ModelState.AddModelError(string.Empty, "Минимальная сумма пополнения — 100 ₽.");
            return View();
        }

        var userId = _userManager.GetUserId(User)!;
        var invoice = new PaymentInvoice
        {
            UserId = userId,
            Amount = amount,
            Provider = PaymentProviders.Test,
            Status = PaymentStatuses.Pending,
            ProviderPaymentId = $"demo-{Guid.NewGuid():N}",
            ConfirmationUrl = $"/Payments/Checkout/0"
        };

        _db.PaymentInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        invoice.ConfirmationUrl = $"/Payments/Checkout/{invoice.Id}";
        await _db.SaveChangesAsync();
        return RedirectToAction("Checkout", "Payments", new { id = invoice.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePaymentInvoice(decimal amount) => await TopUp(amount);

    private async Task<Wallet> GetOrCreateWalletAsync(string userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet is not null) return wallet;
        wallet = new Wallet { UserId = userId, Balance = 0, HoldBalance = 0 };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }
}
