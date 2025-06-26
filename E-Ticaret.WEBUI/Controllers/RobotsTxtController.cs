using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace E_Ticaret.WEBUI.Controllers
{
    public class RobotsTxtController : Controller
    {
        [Route("robots.txt")]
        public ContentResult RobotsTxt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine("Disallow: /admin/");
            sb.AppendLine("Disallow: /account/");
            sb.AppendLine("Disallow: /cart/");
            sb.AppendLine("Disallow: /checkout/");
            sb.AppendLine("Disallow: /search/");
            sb.AppendLine("Disallow: /*?*view=");
            sb.AppendLine("Disallow: /*?*sort=");
            sb.AppendLine("Disallow: /*?*page=\n");
            sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}
