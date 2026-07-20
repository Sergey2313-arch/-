using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviMarket.Web.Data;
using ReviMarket.Web.Models;
using ReviMarket.Web.Services.Payments;

namespace ReviMarket.Web.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentWebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly PaymentLedger _ledger;
    private readonly PaymentReconciliationService _reconciliation;

    public PaymentWebhooksController(
        ApplicationDbContext db,
        PaymentLedger ledger,
        PaymentReconciliationService reconciliation)
    {
        _db = db;
        _ledger = ledger;
        _reconciliation = reconciliation;
    }

    [HttpPost("yookassa")]
    public async Task<IActionResult> YooKassa()
    {
        using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: HttpContext.RequestAborted);
        var root = document.RootElement;

        var eventName = root.TryGetProperty("event", out var eventProperty)
            ? eventProperty.GetString()
            : null;

        if (eventName is not ("payment.succeeded" or "payment.canceled" or "payment.waiting_for_capture"))
        {
            return Ok();
        }

        if (!root.TryGetProperty("object", out var paymentObject)
            || !paymentObject.TryGetProperty("id", out var idProperty))
        {
            return BadRequest();
        }

        var providerPaymentId = idProperty.GetString();
        if (string.IsNullOrWhiteSpace(providerPaymentId))
        {
            return BadRequest();
        }

        var invoice = await _db.PaymentInvoices
            .FirstOrDefaultAsync(
                x => x.Provider == PaymentProviders.YooKassa && x.ProviderPaymentId == providerPaymentId,
                HttpContext.RequestAborted);

        if (invoice is null)
        {
            return Ok();
        }

        var result = await _reconciliation.ReconcileInvoiceAsync(invoice, $"webhook:{eventName}", HttpContext.RequestAborted);
        if (result.Error is not null)
        {
            return result.Error.Contains("configured", StringComparison.OrdinalIgnoreCase)
                ? StatusCode(StatusCodes.Status503ServiceUnavailable)
                : BadRequest();
        }

        return Ok();
    }

    [HttpPost("test-success/{invoiceId:int}")]
    public async Task<IActionResult> TestSuccess(int invoiceId)
    {
        var invoice = await _db.PaymentInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
        if (invoice is null) return NotFound();
        if (invoice.Provider != PaymentProviders.Test) return BadRequest();

        await _ledger.MarkInvoicePaidAsync(invoice, $"Тестовая оплата счета #{invoice.Id}", HttpContext.RequestAborted);
        return Ok();
    }

}
