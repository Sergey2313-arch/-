using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ReviMarket.Web.Models;
using ReviMarket.Web.Options;

namespace ReviMarket.Web.Services.Payments;

public sealed class YooKassaPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly PaymentOptions _options;

    public YooKassaPaymentProvider(HttpClient httpClient, IOptions<PaymentOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.YooKassa.ApiBaseUrl);
    }

    public string Name => PaymentProviders.YooKassa;

    public async Task<PaymentProviderResult> CreatePaymentAsync(PaymentInvoice invoice, Uri returnUrl, CancellationToken cancellationToken)
    {
        if (!_options.IsYooKassaConfigured)
        {
            throw new InvalidOperationException("YooKassa payment provider is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "payments");
        request.Headers.Authorization = CreateAuthHeader();
        request.Headers.Add("Idempotence-Key", $"revimarket-invoice-{invoice.Id}");
        request.Content = JsonContent.Create(new
        {
            amount = new
            {
                value = invoice.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                currency = _options.Currency
            },
            capture = true,
            confirmation = new
            {
                type = "redirect",
                return_url = returnUrl.ToString()
            },
            description = $"Пополнение баланса ReviMarket #{invoice.Id}",
            metadata = new Dictionary<string, string>
            {
                ["invoice_id"] = invoice.Id.ToString(CultureInfo.InvariantCulture),
                ["user_id"] = invoice.UserId
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"YooKassa create payment failed: {(int)response.StatusCode} {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var providerPaymentId = root.GetProperty("id").GetString() ?? throw new InvalidOperationException("YooKassa response does not contain payment id.");
        var providerStatus = root.GetProperty("status").GetString();
        var confirmationUrl = TryGetConfirmationUrl(root);

        return new PaymentProviderResult(providerPaymentId, confirmationUrl, MapStatus(providerStatus));
    }

    public async Task<PaymentProviderStatus?> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        if (!_options.IsYooKassaConfigured || string.IsNullOrWhiteSpace(providerPaymentId))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"payments/{providerPaymentId}");
        request.Headers.Authorization = CreateAuthHeader();

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"YooKassa payment status failed: {(int)response.StatusCode} {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var status = root.GetProperty("status").GetString();
        var paid = root.TryGetProperty("paid", out var paidProperty) && paidProperty.GetBoolean();

        return new PaymentProviderStatus(providerPaymentId, MapStatus(status), paid);
    }

    public static string MapStatus(string? providerStatus) => providerStatus switch
    {
        "succeeded" => PaymentStatuses.Success,
        "canceled" => PaymentStatuses.Failed,
        _ => PaymentStatuses.Pending
    };

    private AuthenticationHeaderValue CreateAuthHeader()
    {
        var raw = $"{_options.YooKassa.ShopId}:{_options.YooKassa.SecretKey}";
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new AuthenticationHeaderValue("Basic", token);
    }

    private static string? TryGetConfirmationUrl(JsonElement root)
    {
        if (!root.TryGetProperty("confirmation", out var confirmation))
        {
            return null;
        }

        return confirmation.TryGetProperty("confirmation_url", out var url)
            ? url.GetString()
            : null;
    }
}
