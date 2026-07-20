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

    public string CurrentName => _options.NormalizedProvider;

    public string? ConfigurationError
    {
        get
        {
            if (CurrentName == PaymentProviders.YooKassa && !_options.IsYooKassaConfigured)
            {
                return "Выбрана YooKassa, но не настроены PAYMENT_SHOP_ID, PAYMENT_SECRET_KEY или адрес API.";
            }

            return null;
        }
    }

    public IPaymentProvider Current
    {
        get
        {
            if (CurrentName == PaymentProviders.YooKassa)
            {
                if (!_options.IsYooKassaConfigured)
                {
                    throw new InvalidOperationException(ConfigurationError);
                }

                return _services.GetRequiredService<YooKassaPaymentProvider>();
            }

            return _services.GetRequiredService<TestPaymentProvider>();
        }
    }
}
