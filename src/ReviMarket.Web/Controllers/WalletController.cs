using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Options;
using ReviMarket.Web.Services.Payments;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PaymentProviderSelector _paymentProviders;
    private readonly PaymentOptions _paymentOptions;

    public WalletController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        PaymentProviderSelector paymentProviders,
        IOptions<PaymentOptions> paymentOptions)
    {
        _db = db;
        _userManager = userManager;
        _paymentProviders = paymentProviders;
        _paymentOptions = paymentOptions.Value;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var wallet = await GetOrCreateWalletAsync(userId);
        var transactions = await _db.PaymentTransactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(15)
            .ToListAsync();
        var invoices = await _db.PaymentInvoices
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.Transactions = transactions;
        ViewBag.Invoices = invoices;
        SetPaymentViewBag();
        return View(wallet);
    }

    [HttpGet]
    public IActionResult TopUp()
    {
        SetPaymentViewBag();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopUp(decimal amount)
    {
        SetPaymentViewBag();

        if (amount < _paymentOptions.MinTopUpAmount)
        {
            ModelState.AddModelError(string.Empty, $"Минимальная сумма пополнения - {_paymentOptions.MinTopUpAmount:N0} ₽.");
            return View();
        }

        if (amount > _paymentOptions.MaxTopUpAmount)
        {
            ModelState.AddModelError(string.Empty, $"Максимальная сумма пополнения - {_paymentOptions.MaxTopUpAmount:N0} ₽.");
            return View();
        }

        if (_paymentProviders.ConfigurationError is string configurationError)
        {
            ModelState.AddModelError(string.Empty, configurationError);
            return View();
        }

        var userId = _userManager.GetUserId(User)!;
        var provider = _paymentProviders.Current;
        var invoice = new PaymentInvoice
        {
            UserId = userId,
            Amount = amount,
            Provider = provider.Name,
            Status = PaymentStatuses.Pending
        };

        _db.PaymentInvoices.Add(invoice);
        await _db.SaveChangesAsync();

        try
        {
            var result = await provider.CreatePaymentAsync(invoice, BuildReturnUri(invoice.Id), HttpContext.RequestAborted);
            invoice.ProviderPaymentId = result.ProviderPaymentId;
            invoice.ConfirmationUrl = result.ConfirmationUrl;
            invoice.Status = result.Status;
        }
        catch
        {
            invoice.Status = PaymentStatuses.Failed;
            await _db.SaveChangesAsync();
            ModelState.AddModelError(string.Empty, "Не удалось создать платеж. Проверь настройки платежного провайдера и попробуй еще раз.");
            return View();
        }

        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(invoice.ConfirmationUrl)
            && Uri.TryCreate(invoice.ConfirmationUrl, UriKind.Absolute, out var confirmationUri))
        {
            return Redirect(confirmationUri.ToString());
        }

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

    private Uri BuildReturnUri(int invoiceId)
    {
        var configuredUrl = _paymentOptions.ReturnUrl;
        var url = string.IsNullOrWhiteSpace(configuredUrl)
            ? Url.Action("Return", "Payments", new { id = invoiceId }, Request.Scheme)!
            : configuredUrl.Replace("{invoiceId}", invoiceId.ToString());

        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        return new Uri($"{Request.Scheme}://{Request.Host}{url}");
    }

    private void SetPaymentViewBag()
    {
        ViewBag.PaymentProvider = _paymentProviders.CurrentName;
        ViewBag.PaymentProviderError = _paymentProviders.ConfigurationError;
        ViewBag.MinTopUpAmount = _paymentOptions.MinTopUpAmount;
        ViewBag.MaxTopUpAmount = _paymentOptions.MaxTopUpAmount;
    }
}
