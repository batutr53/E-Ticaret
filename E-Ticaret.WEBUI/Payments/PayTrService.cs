using E_Ticaret.Core.Entities;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace E_Ticaret.WEBUI.Payments;

public sealed class PayTrService : IPayTrService
{
    private readonly HttpClient _httpClient;
    private readonly PayTrOptions _options;

    public PayTrService(HttpClient httpClient, IOptions<PayTrOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PayTrTokenResult> CreatePaymentTokenAsync(
        Order order,
        string userIp,
        string successUrl,
        string failureUrl,
        CancellationToken cancellationToken = default)
    {
        if (!_options.HasCredentials)
        {
            return new(false, null, "PayTR mağaza bilgileri yapılandırılmamış.");
        }

        var paymentAmount = ToKurus(order.GetTotal());
        var basket = order.OrderItems.Select(item => new object[]
        {
            item.ProductName,
            item.Price.ToString("0.00", CultureInfo.InvariantCulture),
            item.Quantity
        }).ToList();
        if (order.DeliveryFree > 0)
            basket.Add(new object[] { "Teslimat / yol ücreti", order.DeliveryFree.ToString("0.00", CultureInfo.InvariantCulture), 1 });
        var basketJson = JsonSerializer.Serialize(basket);
        var userBasket = Convert.ToBase64String(Encoding.UTF8.GetBytes(basketJson));
        var email = order.SenderEmail ?? string.Empty;
        var noInstallment = _options.NoInstallment.ToString(CultureInfo.InvariantCulture);
        var maxInstallment = _options.MaxInstallment.ToString(CultureInfo.InvariantCulture);
        var testMode = _options.TestMode.ToString(CultureInfo.InvariantCulture);

        var hashValue = string.Concat(
            _options.MerchantId,
            userIp,
            order.Oid,
            email,
            paymentAmount,
            userBasket,
            noInstallment,
            maxInstallment,
            _options.Currency,
            testMode,
            _options.MerchantSalt);

        var formData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["user_ip"] = userIp,
            ["merchant_oid"] = order.Oid ?? string.Empty,
            ["email"] = email,
            ["payment_amount"] = paymentAmount,
            ["paytr_token"] = ComputeToken(hashValue),
            ["user_basket"] = userBasket,
            ["debug_on"] = _options.DebugOn.ToString(CultureInfo.InvariantCulture),
            ["no_installment"] = noInstallment,
            ["max_installment"] = maxInstallment,
            ["user_name"] = $"{order.SenderFirstName} {order.SenderLastName}".Trim(),
            ["user_address"] = $"{order.City} {order.AddresLine}".Trim(),
            ["user_phone"] = order.SenderPhone ?? string.Empty,
            ["merchant_ok_url"] = successUrl,
            ["merchant_fail_url"] = failureUrl,
            ["timeout_limit"] = _options.TimeoutLimit.ToString(CultureInfo.InvariantCulture),
            ["currency"] = _options.Currency,
            ["test_mode"] = testMode,
            ["lang"] = "tr"
        };

        try
        {
            using var response = await _httpClient.PostAsync(
                _options.TokenUrl,
                new FormUrlEncodedContent(formData),
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new(false, null, $"PayTR token servisi HTTP {(int)response.StatusCode} döndürdü.");
            }

            using var json = JsonDocument.Parse(responseBody);
            var root = json.RootElement;
            var status = GetString(root, "status");
            if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
            {
                return new(true, GetString(root, "token"), null);
            }

            return new(false, null, GetString(root, "reason") ?? "PayTR ödeme oturumu oluşturulamadı.");
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new(false, null, "PayTR ödeme servisine şu anda ulaşılamıyor.");
        }
    }

    public async Task<PayTrRefundResult> RefundAsync(
        string merchantOid,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(merchantOid) || amount <= 0 || decimal.Round(amount, 2) != amount)
            return new(false, null, "Geçersiz PayTR sipariş numarası veya iade tutarı.");
        if (!_options.HasCredentials)
        {
            return new(false, null, "PayTR mağaza bilgileri yapılandırılmamış.");
        }

        var returnAmount = amount.ToString("0.00", CultureInfo.InvariantCulture);
        var tokenInput = string.Concat(
            _options.MerchantId,
            merchantOid,
            returnAmount,
            _options.MerchantSalt);
        var formData = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["merchant_oid"] = merchantOid,
            ["return_amount"] = returnAmount,
            ["paytr_token"] = ComputeToken(tokenInput)
        };

        try
        {
            using var response = await _httpClient.PostAsync(
                _options.RefundUrl,
                new FormUrlEncodedContent(formData),
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new(false, null, $"PayTR iade servisi HTTP {(int)response.StatusCode} döndürdü.", true);
            }

            using var json = JsonDocument.Parse(responseBody);
            var root = json.RootElement;
            if (string.Equals(GetString(root, "status"), "success", StringComparison.OrdinalIgnoreCase))
            {
                if (GetString(root, "merchant_oid") != merchantOid ||
                    !decimal.TryParse(GetString(root, "return_amount"), NumberStyles.Number, CultureInfo.InvariantCulture, out var returnedAmount) ||
                    returnedAmount != amount)
                    return new(false, null, "PayTR iade yanıtındaki sipariş/tutar doğrulanamadı.", true);
                return new(true, null, null);
            }

            var status = GetString(root, "status");
            return new(false, GetString(root, "err_no"), GetString(root, "err_msg") ?? "PayTR iade işlemi doğrulanamadı.",
                status is not ("error" or "failed"));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new(false, null, "PayTR iade servisine şu anda ulaşılamıyor; sonuç kontrol edilmeli.", true);
        }
    }

    public bool ValidateCallback(PayTrCallbackModel callback)
    {
        if (!_options.HasCredentials ||
            string.IsNullOrWhiteSpace(callback.MerchantOid) ||
            string.IsNullOrWhiteSpace(callback.Status) ||
            string.IsNullOrWhiteSpace(callback.TotalAmount) ||
            string.IsNullOrWhiteSpace(callback.Hash))
        {
            return false;
        }

        var tokenInput = string.Concat(
            callback.MerchantOid,
            _options.MerchantSalt,
            callback.Status,
            callback.TotalAmount);

        try
        {
            var expected = Convert.FromBase64String(ComputeToken(tokenInput));
            var received = Convert.FromBase64String(callback.Hash);
            return expected.Length == received.Length &&
                   CryptographicOperations.FixedTimeEquals(expected, received);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public string GetPaymentUrl(string token) =>
        $"{_options.PaymentUrl.TrimEnd('/')}/{Uri.EscapeDataString(token)}";

    private string ComputeToken(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.MerchantKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string ToKurus(decimal amount) =>
        decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero)
            .ToString("0", CultureInfo.InvariantCulture);

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }
}
