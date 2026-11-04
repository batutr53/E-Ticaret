using System.Net;
using System.Text.Json;

namespace E_Ticaret.WEBUI.Helpers;

public class TelegramHelper(HttpClient client, IConfiguration configuration)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["TelegramBot:BotToken"])
        && !string.IsNullOrWhiteSpace(configuration["TelegramBot:ChatId"]);

    public Task SendTelegramMessage(string message) => SendAsync(message, false, CancellationToken.None);

    public Task SendTelegramOrderMessage(int orderId, string message, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["TelegramBot:AdminPanelBaseUrl"];
        var html = WebUtility.HtmlEncode(message);
        if (Uri.TryCreate($"{baseUrl}{orderId}", UriKind.Absolute, out var url)
            && url.Scheme is "https" or "http")
            html += $" <a href=\"{WebUtility.HtmlEncode(url.AbsoluteUri)}\">Sipariş detayını aç</a>";
        return SendAsync(html, true, cancellationToken);
    }

    private async Task SendAsync(string message, bool html, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new InvalidOperationException("Telegram bot veya sohbet ayarı eksik.");
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = configuration["TelegramBot:ChatId"]!,
            ["text"] = message,
            ["parse_mode"] = html ? "HTML" : "",
            ["disable_web_page_preview"] = "true"
        });
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync($"https://api.telegram.org/bot{configuration["TelegramBot:BotToken"]}/sendMessage", content, cancellationToken);
        }
        catch (HttpRequestException)
        {
            // Never expose request URLs containing the bot token.
            throw new InvalidOperationException("Telegram sunucusuna ulaşılamadı.");
        }
        using (response)
        {
            try
            {
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                EnsureAccepted(response, body.RootElement);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Telegram geçersiz yanıt döndürdü.");
            }
        }
    }

    private static void EnsureAccepted(HttpResponseMessage response, JsonElement body)
    {
        if (response.IsSuccessStatusCode && body.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.True) return;
        var code = body.TryGetProperty("error_code", out var value) && value.TryGetInt32(out var parsed)
            ? parsed : (int)response.StatusCode;
        var description = body.TryGetProperty("description", out var reason) && reason.ValueKind == JsonValueKind.String ? reason.GetString() : null;
        throw new TelegramDeliveryException(code, description);
    }

    // Read-only checks; never sends a test notification to the customer's chat.
    public async Task<string> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) return "Telegram bot veya sohbet ayarı eksik.";
        try
        {
            foreach (var method in new[] { "getMe", "getChat" })
            {
                using var content = new FormUrlEncodedContent(method == "getChat"
                    ? new Dictionary<string, string> { ["chat_id"] = configuration["TelegramBot:ChatId"]! }
                    : new Dictionary<string, string>());
                using var response = await client.PostAsync($"https://api.telegram.org/bot{configuration["TelegramBot:BotToken"]}/{method}", content, cancellationToken);
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                EnsureAccepted(response, body.RootElement);
            }
            return "Bot anahtarı geçerli ve sohbet erişilebilir. Mesaj gönderme yetkisi ayrıca gereklidir; test mesajı gönderilmedi.";
        }
        catch (TelegramDeliveryException exception) { return exception.Message; }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            return "Telegram bağlantısı doğrulanamadı. Sunucu bağlantısını kontrol edin.";
        }
    }
}
