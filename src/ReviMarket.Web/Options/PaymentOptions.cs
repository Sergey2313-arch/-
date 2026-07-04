using ReviMarket.Web.Models;

namespace ReviMarket.Web.Options;

public class PaymentOptions
{
    public string Provider { get; set; } = PaymentProviders.Test;

    public string Currency { get; set; } = "RUB";

    public string ReturnUrl { get; set; } = string.Empty;

    public decimal MinTopUpAmount { get; set; } = 100m;

    public decimal MaxTopUpAmount { get; set; } = 500000m;

    public YooKassaOptions YooKassa { get; set; } = new();

    public bool IsYooKassaConfigured =>
        string.Equals(Provider, PaymentProviders.YooKassa, StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(YooKassa.ShopId)
        && !string.IsNullOrWhiteSpace(YooKassa.SecretKey)
        && Uri.TryCreate(YooKassa.ApiBaseUrl, UriKind.Absolute, out _);

    public string NormalizedProvider =>
        string.Equals(Provider, PaymentProviders.YooKassa, StringComparison.OrdinalIgnoreCase)
            ? PaymentProviders.YooKassa
            : PaymentProviders.Test;
}

public class YooKassaOptions
{
    public string ShopId { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "https://api.yookassa.ru/v3/";
}
