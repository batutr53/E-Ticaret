using System;
using System.Text;
using System.Text.RegularExpressions;

namespace E_Ticaret.WEBUI.Extensions
{
    public static class UrlExtensions
    {
        public static string ToUrlFriendly(this string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            // Tüm harfleri küçük yap
            text = text.ToLowerInvariant();

            // Türkçe karakterleri değiştir
            text = text
                .Replace('ü', 'u')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ş', 's');

            // URL'de izin verilmeyen karakterleri kaldır
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            
            // Boşlukları tireye çevir ve fazla boşlukları temizle
            text = Regex.Replace(text, @"\s+", " ").Trim();
            text = text.Replace(" ", "-");
            
            // Ardışık tireleri tek tireye düşür
            text = Regex.Replace(text, @"-+", "-");

            return text;
        }

        public static string ToSeoUrl(this string text, int maxLength = 70)
        {
            if (string.IsNullOrEmpty(text)) return "";

            var friendlyUrl = text.ToUrlFriendly();
            
            // Maksimum uzunluk kontrolü
            if (friendlyUrl.Length > maxLength)
            {
                friendlyUrl = friendlyUrl.Substring(0, maxLength);
                
                // Son karakterin tire olmamasına dikkat et
                if (friendlyUrl.EndsWith("-"))
                    friendlyUrl = friendlyUrl.Substring(0, friendlyUrl.Length - 1);
            }

            return friendlyUrl;
        }
    }
}
