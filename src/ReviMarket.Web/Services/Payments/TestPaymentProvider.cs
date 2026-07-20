using ReviMarket.Web.Models;

namespace ReviMarket.Web.Services.Payments;

public sealed class TestPaymentProvider : IPaymentProvider
{
    public string Name => PaymentProviders.Test;

    public Task<PaymentProviderResult> CreatePaymentAsync(PaymentInvoice invoice, Uri returnUrl, CancellationToken cancellationToken)
    {
        var providerPaymentId = $"demo-{Guid.NewGuid():N}";
        var checkoutUrl = $"/Payments/Checkout/{invoice.Id}";
        return Task.FromResult(new PaymentProviderResult(providerPaymentId, checkoutUrl, PaymentStatuses.Pending));
    }

    public Task<PaymentProviderStatus?> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        return Task.FromResult<PaymentProviderStatus?>(null);
    }
}
