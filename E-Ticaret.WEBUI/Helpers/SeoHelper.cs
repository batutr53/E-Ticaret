using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;

namespace E_Ticaret.WEBUI.Helpers
{
    public static class SeoHelper
    {
        public static HtmlString GenerateMetaTags(
            this IHtmlHelper html,
            string title,
            string description,
            string keywords = null,
            string imageUrl = null,
            string url = null,
            string type = "website",
            string siteName = "Detay Çiçekçilik",
            string locale = "tr_TR")
        {
            if (string.IsNullOrEmpty(url))
            {
                var request = html.ViewContext.HttpContext.Request;
                url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                var request = html.ViewContext.HttpContext.Request;
                imageUrl = $"{request.Scheme}://{request.Host}/images/logo.png";
            }

            var meta = new StringBuilder();

            // Title
            meta.AppendLine($"<title>{title} - {siteName}</title>");

            // Basic Meta Tags
            meta.AppendLine($"<meta name=\"description\" content=\"{description}\">");
            
            if (!string.IsNullOrEmpty(keywords))
            {
                meta.AppendLine($"<meta name=\"keywords\" content=\"{keywords}\">");
            }

            // Open Graph / Facebook
            meta.AppendLine($"<meta property=\"og:type\" content=\"{type}\">");
            meta.AppendLine($"<meta property=\"og:url\" content=\"{url}\">");
            meta.AppendLine($"<meta property=\"og:title\" content=\"{title}\">");
            meta.AppendLine($"<meta property=\"og:description\" content=\"{description}\">");
            meta.AppendLine($"<meta property=\"og:image\" content=\"{imageUrl}\">");
            meta.AppendLine($"<meta property=\"og:site_name\" content=\"{siteName}\">");
            meta.AppendLine($"<meta property=\"og:locale\" content=\"{locale}\">");

            // Twitter Card
            meta.AppendLine($"<meta name=\"twitter:card\" content=\"summary_large_image\">");
            meta.AppendLine($"<meta name=\"twitter:url\" content=\"{url}\">");
            meta.AppendLine($"<meta name=\"twitter:title\" content=\"{title}\">");
            meta.AppendLine($"<meta name=\"twitter:description\" content=\"{description}\">");
            meta.AppendLine($"<meta name=\"twitter:image\" content=\"{imageUrl}\">");

            // Canonical URL
            meta.AppendLine($"<link rel=\"canonical\" href=\"{url}\" />");

            // Favicon
            meta.AppendLine("<link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/favicon-32x32.png\">");
            meta.AppendLine("<link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"/favicon-16x16.png\">");
            meta.AppendLine("<link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"/apple-touch-icon.png\">");
            meta.AppendLine("<link rel=\"manifest\" href=\"/site.webmanifest\">");
            meta.AppendLine("<meta name=\"theme-color\" content=\"#ffffff\">");

            return new HtmlString(meta.ToString());
        }

        public static HtmlString GenerateBreadcrumbJsonLd(this IHtmlHelper html, string[] breadcrumbs)
        {
            if (breadcrumbs == null || breadcrumbs.Length == 0)
                return HtmlString.Empty;

            var items = new StringBuilder();
            var position = 1;
            var request = html.ViewContext.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            foreach (var item in breadcrumbs)
            {
                if (position > 1) items.Append(",");
                items.Append($"{{\"@type\":\"ListItem\",\"position\":{position},\"name\":\"{item}\",\"item\":\"{baseUrl}/{item.ToLower().Replace(" ", "-")}\"}}");
                position++;
            }

            var jsonLd = $@"
        <script type=""application/ld+json"">
        {{
            ""@context"": ""https://schema.org"",
            ""@type"": ""BreadcrumbList"",
            ""itemListElement"": [{items}]
        }}
        </script>";

            return new HtmlString(jsonLd);
        }
    }
}
