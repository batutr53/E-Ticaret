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
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Telegram bildirimi reddedildi (HTTP {(int)response.StatusCode}).");
            try
            {
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                if (!body.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True)
                    throw new InvalidOperationException("Telegram bildirimi kabul edilmedi.");
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Telegram geçersiz yanıt döndürdü.");
            }
        }
    }
}
