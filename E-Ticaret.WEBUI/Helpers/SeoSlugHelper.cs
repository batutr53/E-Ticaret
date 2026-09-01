using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace E_Ticaret.WEBUI.Helpers
{
    public static class SeoSlugHelper
    {
        public static string Generate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().ToLower(new CultureInfo("tr-TR"));
            normalized = normalized
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c");

            normalized = new string(normalized
                .Normalize(NormalizationForm.FormD)
                .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Normalize(NormalizationForm.FormC);

            normalized = Regex.Replace(normalized, "[^a-z0-9]+", "-");
            return normalized.Trim('-');
        }
    }
}
