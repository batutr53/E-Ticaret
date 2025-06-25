using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace E_Ticaret.WEBUI.Controllers
{
    [Route("[controller]")]
    public class SitemapController : Controller
    {
        private readonly IMemoryCache _cache;

        public SitemapController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet("sitemap.xml")]
        public async Task<IActionResult> SitemapXml()
        {
            string xml = await _cache.GetOrCreateAsync("Sitemap", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return await GenerateSitemap();
            });

            return Content(xml, "application/xml");
        }

        private async Task<string> GenerateSitemap()
        {
            var urls = new List<SitemapUrl>
            {
                new SitemapUrl { 
                    Url = "", 
                    LastModified = DateTime.UtcNow, 
                    ChangeFrequency = SitemapChangeFrequency.Daily, 
                    Priority = 1.0 
                },
                new SitemapUrl { 
                    Url = "hakkimizda", 
                    LastModified = DateTime.UtcNow, 
                    ChangeFrequency = SitemapChangeFrequency.Monthly, 
                    Priority = 0.8 
                },
                new SitemapUrl { 
                    Url = "urunler", 
                    LastModified = DateTime.UtcNow, 
                    ChangeFrequency = SitemapChangeFrequency.Daily, 
                    Priority = 0.9 
                },
                new SitemapUrl { 
                    Url = "iletisim", 
                    LastModified = DateTime.UtcNow, 
                    ChangeFrequency = SitemapChangeFrequency.Yearly, 
                    Priority = 0.7 
                }
                // Buraya diğer sayfalarınızı ekleyebilirsiniz
            };

            var sitemapNodes = urls.Select(u => new SitemapNode
            {
                Url = u.Url,
                LastMod = u.LastModified,
                Frequency = u.ChangeFrequency,
                Priority = u.Priority
            }).ToList();

            return await Task.Run(() =>
            {
                var xmlDocument = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("urlset",
                        new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                        from item in sitemapNodes
                        select CreateItemElement(item)
                    )
                );

                var xml = xmlDocument.Declaration + Environment.NewLine + xmlDocument;
                return xml;
            });
        }

        private XElement CreateItemElement(SitemapNode node)
        {
            XElement itemElement = new XElement("url");
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            itemElement.Add(new XElement("loc", Uri.EscapeUriString($"{baseUrl}/{node.Url.TrimStart('/')}")));
            itemElement.Add(new XElement("lastmod", node.LastMod.ToString("yyyy-MM-ddTHH:mm:sszzz")));
            itemElement.Add(new XElement("changefreq", node.Frequency.ToString().ToLower()));
            itemElement.Add(new XElement("priority", node.Priority.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)));
            return itemElement;
        }
    }

    public class SitemapNode
    {
        public string Url { get; set; }
        public DateTime LastMod { get; set; }
        public SitemapChangeFrequency Frequency { get; set; }
        public double Priority { get; set; }
    }

    public class SitemapUrl
    {
        public string Url { get; set; }
        public DateTime LastModified { get; set; }
        public SitemapChangeFrequency ChangeFrequency { get; set; }
        public double Priority { get; set; }
    }

    public enum SitemapChangeFrequency
    {
        Always,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }
}
