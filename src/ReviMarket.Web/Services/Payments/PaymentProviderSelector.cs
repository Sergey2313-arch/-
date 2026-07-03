using Microsoft.Extensions.Options;
using ReviMarket.Web.Models;
using ReviMarket.Web.Options;

namespace ReviMarket.Web.Services.Payments;

public sealed class PaymentProviderSelector
{
    private readonly IServiceProvider _services;
    private readonly PaymentOptions _options;

    public PaymentProviderSelector(IServiceProvider services, IOptions<PaymentOptions> options)
    {
        _services = services;
        _options = options.Value;
    }

    public IPaymentProvider Current
    {
        get
        {
            if (string.Equals(_options.Provider, PaymentProviders.YooKassa, StringComparison.OrdinalIgnoreCase))
            {
                return _options.IsYooKassaConfigured
                    ? _services.GetRequiredService<YooKassaPaymentProvider>()
                    : _services.GetRequiredService<TestPaymentProvider>();
            }

            return _services.GetRequiredService<TestPaymentProvider>();
        }
    }
}
