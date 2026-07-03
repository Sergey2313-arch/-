using ReviMarket.Web.Models;

namespace ReviMarket.Web.Services.Payments;

public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentProviderResult> CreatePaymentAsync(PaymentInvoice invoice, Uri returnUrl, CancellationToken cancellationToken);

    Task<PaymentProviderStatus?> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken);
}
