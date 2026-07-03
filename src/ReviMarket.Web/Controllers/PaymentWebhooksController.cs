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
    private readonly PaymentProviderSelector _paymentProviders;
    private readonly PaymentLedger _ledger;

    public PaymentWebhooksController(
        ApplicationDbContext db,
        PaymentProviderSelector paymentProviders,
        PaymentLedger ledger)
    {
        _db = db;
        _paymentProviders = paymentProviders;
        _ledger = ledger;
    }

    [HttpPost("yookassa")]
    public async Task<IActionResult> YooKassa()
    {
        using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: HttpContext.RequestAborted);
        var root = document.RootElement;

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

        var status = await ResolveProviderStatusAsync(providerPaymentId, paymentObject);
        if (status == PaymentStatuses.Success)
        {
            await _ledger.MarkInvoicePaidAsync(invoice, $"Оплата счета #{invoice.Id} через ЮKassa", HttpContext.RequestAborted);
        }
        else if (status == PaymentStatuses.Failed)
        {
            await _ledger.MarkInvoiceFailedAsync(invoice, HttpContext.RequestAborted);
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

    private async Task<string> ResolveProviderStatusAsync(string providerPaymentId, JsonElement paymentObject)
    {
        var provider = _paymentProviders.Current;
        if (provider.Name == PaymentProviders.YooKassa)
        {
            var remoteStatus = await provider.GetPaymentAsync(providerPaymentId, HttpContext.RequestAborted);
            if (remoteStatus is not null)
            {
                return remoteStatus.Status;
            }
        }

        var payloadStatus = paymentObject.TryGetProperty("status", out var statusProperty)
            ? statusProperty.GetString()
            : null;

        return YooKassaPaymentProvider.MapStatus(payloadStatus);
    }
}
