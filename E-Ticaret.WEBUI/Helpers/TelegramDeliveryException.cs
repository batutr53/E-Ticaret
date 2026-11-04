namespace E_Ticaret.WEBUI.Helpers;

// Only fixed descriptions are persisted; never include a bot URL, token or API response.
public sealed class TelegramDeliveryException(int errorCode, string? description = null)
    : InvalidOperationException(Describe(errorCode, description))
{
    private static string Describe(int code, string? description) => code switch
    {
        401 => "Telegram bot anahtarı geçersiz veya iptal edilmiş (401). BotFather üzerinden anahtarı yenileyip sunucu ayarını güncelleyin.",
        403 => "Telegram botunun sohbete erişimi yok veya bot engellenmiş (403). Bot üyeliğini ve mesaj gönderme yetkisini kontrol edin.",
        429 => "Telegram gönderim sınırına ulaşıldı (429). Bildirim daha sonra yeniden denenecek.",
        400 when description?.Contains("chat not found", StringComparison.OrdinalIgnoreCase) == true
            => "Telegram sohbeti bulunamadı (400). Sohbet numarasını ve botun gruba ekli olduğunu kontrol edin.",
        400 when description?.Contains("migrat", StringComparison.OrdinalIgnoreCase) == true
            => "Telegram grubu değişmiş (400). Sunucudaki sohbet numarası güncellenmeli.",
        400 => "Telegram isteği reddedildi (400). Sohbet ve bildirim biçimi kontrol edilmeli.",
        _ => $"Telegram bildirimi reddedildi (HTTP {code})."
    };
}
