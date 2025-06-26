using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Web;

namespace E_Ticaret.WEBUI.TagHelpers
{
    [HtmlTargetElement("seo-meta", TagStructure = TagStructure.WithoutEndTag)]
    public class SeoMetaTagHelper : TagHelper
    {
        [HtmlAttributeName("title")]
        public string Title { get; set; } = "E-Ticaret Sitesi";

        [HtmlAttributeName("description")]
        public string Description { get; set; } = "En uygun fiyatlı ürünler burada!";

        [HtmlAttributeName("keywords")]
        public string Keywords { get; set; }

        [HtmlAttributeName("image")]
        public string ImageUrl { get; set; }

        [HtmlAttributeName("type")]
        public string Type { get; set; } = "website";

        [HtmlAttributeName("twitter-card")]
        public string TwitterCard { get; set; } = "summary_large_image";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public SeoMetaTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string ToFullImageUrl(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
                return "/img/no-image.png";

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return relativeImagePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{(relativeImagePath.StartsWith("/") ? "" : "/")}{relativeImagePath.TrimStart('/')}";
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // <seo-meta> etiketini kaldır
            output.TagMode = TagMode.StartTagAndEndTag;

            var sb = new StringBuilder();

            // Temel Meta
            sb.AppendLine($"<title>{HttpUtility.HtmlEncode(Title)}</title>");
            sb.AppendLine($"<meta name=\"description\" content=\"{HttpUtility.HtmlEncode(Description)}\" />");
            
            if (!string.IsNullOrEmpty(Keywords))
            {
                sb.AppendLine($"<meta name=\"keywords\" content=\"{HttpUtility.HtmlEncode(Keywords)}\" />");
            }

            // Open Graph
            sb.AppendLine($"<meta property=\"og:title\" content=\"{HttpUtility.HtmlEncode(Title)}\" />");
            sb.AppendLine($"<meta property=\"og:description\" content=\"{HttpUtility.HtmlEncode(Description)}\" />");
            sb.AppendLine($"<meta property=\"og:type\" content=\"{Type}\" />");
            sb.AppendLine($"<meta property=\"og:url\" content=\"{GetCurrentUrl()}\" />");
            
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                var fullImageUrl = ToFullImageUrl(ImageUrl);
                sb.AppendLine($"<meta property=\"og:image\" content=\"{fullImageUrl}\" />");
                sb.AppendLine($"<meta property=\"og:image:alt\" content=\"{HttpUtility.HtmlEncode(Title)}\" />");
            }

            // Twitter Card
            sb.AppendLine($"<meta name=\"twitter:card\" content=\"{TwitterCard}\" />");
            sb.AppendLine($"<meta name=\"twitter:title\" content=\"{HttpUtility.HtmlEncode(Title)}\" />");
            sb.AppendLine($"<meta name=\"twitter:description\" content=\"{HttpUtility.HtmlEncode(Description)}\" />");
            
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                var fullImageUrl = ToFullImageUrl(ImageUrl);
                sb.AppendLine($"<meta name=\"twitter:image\" content=\"{fullImageUrl}\" />");
                sb.AppendLine($"<meta name=\"twitter:image:alt\" content=\"{HttpUtility.HtmlEncode(Title)}\" />");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }

        private string GetCurrentUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;
            
            return $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        }
    }
}
