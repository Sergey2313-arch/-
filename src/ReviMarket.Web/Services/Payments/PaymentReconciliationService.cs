using Microsoft.Extensions.Options;
using ReviMarket.Web.Models;
using ReviMarket.Web.Options;

namespace ReviMarket.Web.Services.Payments;

public sealed class PaymentReconciliationService
{
    private readonly PaymentProviderSelector _paymentProviders;
    private readonly PaymentLedger _ledger;
    private readonly PaymentOptions _options;
    private readonly ILogger<PaymentReconciliationService> _logger;

    public PaymentReconciliationService(
        PaymentProviderSelector paymentProviders,
        PaymentLedger ledger,
        IOptions<PaymentOptions> options,
        ILogger<PaymentReconciliationService> logger)
    {
        _paymentProviders = paymentProviders;
        _ledger = ledger;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentReconciliationResult> ReconcileInvoiceAsync(
        PaymentInvoice invoice,
        string source,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(invoice.ProviderPaymentId))
        {
            return new PaymentReconciliationResult(invoice.Status, false, "Invoice has no provider payment id.");
        }

        IPaymentProvider provider;
        try
        {
            provider = _paymentProviders.Current;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment provider is not configured while reconciling invoice {InvoiceId}", invoice.Id);
            return new PaymentReconciliationResult(invoice.Status, false, ex.Message);
        }

        if (!string.Equals(provider.Name, invoice.Provider, StringComparison.OrdinalIgnoreCase))
        {
            return new PaymentReconciliationResult(invoice.Status, false, "Configured payment provider does not match invoice provider.");
        }

        var status = await provider.GetPaymentAsync(invoice.ProviderPaymentId, cancellationToken);
        if (status is null)
        {
            return new PaymentReconciliationResult(invoice.Status, false, "Provider did not return payment status.");
        }

        if (status.Status == PaymentStatuses.Success)
        {
            var validationError = ValidateProviderStatus(invoice, status);
            if (validationError is not null)
            {
                _logger.LogWarning(
                    "Payment verification failed for invoice {InvoiceId}: {Error}",
                    invoice.Id,
                    validationError);
                return new PaymentReconciliationResult(invoice.Status, false, validationError);
            }

            var changed = await _ledger.MarkInvoicePaidAsync(
                invoice,
                $"Оплата счета #{invoice.Id} через {invoice.Provider} ({source})",
                cancellationToken);

            return new PaymentReconciliationResult(PaymentStatuses.Success, changed);
        }

        if (status.Status == PaymentStatuses.Failed)
        {
            await _ledger.MarkInvoiceFailedAsync(invoice, cancellationToken);
            return new PaymentReconciliationResult(PaymentStatuses.Failed, true);
        }

        return new PaymentReconciliationResult(PaymentStatuses.Pending, false);
    }

    private string? ValidateProviderStatus(PaymentInvoice invoice, PaymentProviderStatus status)
    {
        if (!string.Equals(status.ProviderPaymentId, invoice.ProviderPaymentId, StringComparison.Ordinal))
        {
            return "Provider payment id does not match invoice.";
        }

        if (invoice.Provider == PaymentProviders.YooKassa)
        {
            if (status.Amount is null)
            {
                return "YooKassa payment amount is missing.";
            }

            if (status.Amount.Value != invoice.Amount)
            {
                return "YooKassa payment amount does not match invoice amount.";
            }

            if (!string.Equals(status.Currency, _options.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return "YooKassa payment currency does not match configured currency.";
            }

            var expectedInvoiceId = invoice.Id.ToString();
            if (!string.Equals(status.InvoiceId, expectedInvoiceId, StringComparison.Ordinal))
            {
                return "YooKassa payment metadata invoice_id does not match invoice.";
            }
        }

        return null;
    }
}
