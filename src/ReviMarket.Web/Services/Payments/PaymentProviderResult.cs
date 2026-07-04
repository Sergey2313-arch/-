namespace ReviMarket.Web.Services.Payments;

public sealed record PaymentProviderResult(
    string ProviderPaymentId,
    string? ConfirmationUrl,
    string Status);

public sealed record PaymentProviderStatus(
    string ProviderPaymentId,
    string Status,
    bool Paid,
    decimal? Amount = null,
    string? Currency = null,
    string? InvoiceId = null);

public sealed record PaymentReconciliationResult(
    string? Status,
    bool Changed,
    string? Error = null);
