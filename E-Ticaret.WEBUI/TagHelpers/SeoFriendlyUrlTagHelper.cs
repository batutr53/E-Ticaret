using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace E_Ticaret.WEBUI.TagHelpers
{
    [HtmlTargetElement("seo-friendly-url", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SeoFriendlyUrlTagHelper : TagHelper
    {
        [HtmlAttributeName("controller")]
        public string Controller { get; set; }

        [HtmlAttributeName("action")]
        public string Action { get; set; }

        [HtmlAttributeName("id")]
        public int? Id { get; set; }


        [HtmlAttributeName("seo-text")]
        public string SeoText { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            
            // URL oluştur
            var url = new StringBuilder();
            
            if (!string.IsNullOrEmpty(Controller))
            {
                url.Append("/").Append(Controller.ToLower());
            }
            
            if (!string.IsNullOrEmpty(Action) && !Action.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                url.Append("/").Append(Action.ToLower());
            }
            
            if (Id.HasValue && Id.Value > 0)
            {
                url.Append("/").Append(Id.Value);
                
                // SEO metni ekle
                if (!string.IsNullOrEmpty(SeoText))
                {
                    url.Append("/").Append(GenerateSeoUrl(SeoText));
                }
            }
            
            output.Attributes.SetAttribute("href", url.ToString());
        }
        
        private string GenerateSeoUrl(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            // Tüm harfleri küçük yap
            var url = text.ToLowerInvariant();

            // Türkçe karakterleri değiştir
            url = url
                .Replace('ü', 'u')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ş', 's')
                .Replace(' ', '-')
                .Replace("-", "-")
                .Replace("&", "ve");

            // URL'de izin verilmeyen karakterleri kaldır
            url = Regex.Replace(url, @"[^a-z0-9\-]", "");
            
            // Ardışık tireleri tek tireye düşür
            url = Regex.Replace(url, @"-+", "-");
            
            // Başta ve sonda tire varsa kaldır
            url = url.Trim('-');

            return url;
        }
    }
}
