using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace E_Ticaret.WEBUI.Helpers;

public static partial class RichTextHelper
{
    // Decode Turkish entities only; leave intentional HTML examples such as &lt;script&gt; as text.
    public static string Normalize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        if (EncodedDocument().IsMatch(html)) html = WebUtility.HtmlDecode(html);
        html = TurkishEntity().Replace(html, match =>
        {
            var value = match.Value;
            for (var i = 0; i < 5; i++)
            {
                var decoded = WebUtility.HtmlDecode(value);
                if (decoded == value) break;
                value = decoded;
            }
            return value;
        });
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Remove("style");
        return sanitizer.Sanitize(html);
    }

    [GeneratedRegex(@"^\s*&lt;(?:p|h[1-6]|div|ul|ol|blockquote)(?:&gt;|\s)", RegexOptions.IgnoreCase)]
    private static partial Regex EncodedDocument();

    [GeneratedRegex(@"&(?:amp;){0,4}(?:ouml|Ouml|uuml|Uuml|ccedil|Ccedil|#(?:231|199|246|214|252|220|287|286|351|350|305|304)|#x(?:e7|c7|f6|d6|fc|dc|11f|11e|15f|15e|131|130));", RegexOptions.IgnoreCase)]
    private static partial Regex TurkishEntity();
}
