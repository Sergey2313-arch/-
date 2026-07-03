namespace ReviMarket.Web.Services.Payments;

public sealed record PaymentProviderResult(
    string ProviderPaymentId,
    string? ConfirmationUrl,
    string Status);

public sealed record PaymentProviderStatus(
    string ProviderPaymentId,
    string Status,
    bool Paid);
