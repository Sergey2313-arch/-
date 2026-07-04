using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Services.Payments;

namespace ReviMarket.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly PaymentLedger _ledger;
    private readonly PaymentReconciliationService _reconciliation;

    public PaymentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        PaymentLedger ledger,
        PaymentReconciliationService reconciliation)
    {
        _db = db;
        _users = users;
        _ledger = ledger;
        _reconciliation = reconciliation;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
        if (invoice is null) return NotFound();

        if (invoice.Status == PaymentStatuses.Pending && !string.IsNullOrWhiteSpace(invoice.ProviderPaymentId))
        {
            await _reconciliation.ReconcileInvoiceAsync(invoice, "checkout", HttpContext.RequestAborted);
            invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (invoice is null) return NotFound();
        }

        ViewBag.CanUseDemoConfirmation = invoice.Provider == PaymentProviders.Test;
        return View(invoice);
    }

    [HttpGet]
    public async Task<IActionResult> Return(int id)
    {
        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
        if (invoice is null) return NotFound();

        if (invoice.Status == PaymentStatuses.Pending && !string.IsNullOrWhiteSpace(invoice.ProviderPaymentId))
        {
            await _reconciliation.ReconcileInvoiceAsync(invoice, "return", HttpContext.RequestAborted);
        }

        return RedirectToAction(nameof(Checkout), new { id = invoice.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDemo(int id)
    {
        var uid = _users.GetUserId(User)!;
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
        if (invoice is null) return NotFound();
        if (invoice.Provider != PaymentProviders.Test) return BadRequest();

        await _ledger.MarkInvoicePaidAsync(invoice, $"Тестовая оплата счета #{invoice.Id}", HttpContext.RequestAborted);
        return RedirectToAction("Index", "Wallet");
    }
}
