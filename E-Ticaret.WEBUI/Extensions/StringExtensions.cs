using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace E_Ticaret.WEBUI.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrlFriendly(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Tüm harfleri küçük yap
            text = text.ToLowerInvariant();

            // Türkçe karakterleri dönüştür
            text = text
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c");

            // Özel karakterleri kaldır
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            
            // Birden fazla boşluğu tek boşluğa çevir
            text = Regex.Replace(text, @"\s+", " ").Trim();
            
            // Boşlukları tireye çevir
            text = text.Replace(" ", "-");
            
            // Birden fazla tireyi tek tireye çevir
            text = Regex.Replace(text, @"-+", "-");
            
            return text;
        }
    }
}
