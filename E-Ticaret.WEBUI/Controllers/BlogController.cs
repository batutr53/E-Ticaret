using E_Ticaret.Data;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;

namespace E_Ticaret.WEBUI.Controllers;

[Route("blog")]
public class BlogController(DatabaseContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? category, string? q, int page = 1)
    {
        if (page < 1) return NotFound();
        q = q?.Trim();
        if (q?.Length > 100) q = q[..100];
        var query = db.BlogPosts.AsNoTracking().Include(x => x.BlogCategory).Published(DateTime.UtcNow);
        if (!string.IsNullOrEmpty(category))
        {
            if (!await db.BlogCategories.AnyAsync(x => x.IsActive && x.Slug == category)) return NotFound();
            query = query.Where(x => x.BlogCategory!.Slug == category);
        }
        if (!string.IsNullOrEmpty(q))
        {
            var search = q.ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(search) || x.Excerpt.ToLower().Contains(search));
        }
        var total = await query.CountAsync();
        if (page > Math.Max(1, (int)Math.Ceiling(total / 12d))) return NotFound();
        var model = new BlogIndexViewModel
        {
            Settings = await db.BlogSettings.AsNoTracking().FirstOrDefaultAsync() ?? new(),
            Categories = await db.BlogCategories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(),
            Posts = await query.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id).Skip((page - 1) * 12).Take(12).ToListAsync(),
            Search = q, CategorySlug = category, Page = page, Total = total
        };
        ViewData["SeoDescription"] = model.Settings.Description;
        ViewData["Robots"] = !string.IsNullOrEmpty(q) ? "noindex,follow" : "index,follow";
        ViewData["CanonicalPath"] = Url.Action(nameof(Index), new { category, page = page > 1 ? page : (int?)null });
        return View(model);
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        // Stable configured site origin; never trust a request Host header in SEO output.
        var origin = "https://www.detaycicek.com";
        var posts = await db.BlogPosts.AsNoTracking().Published(DateTime.UtcNow).OrderBy(x => x.Id).Select(x => new { x.Slug, x.UpdatedAt }).ToListAsync();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var root = new XElement(ns + "urlset", new XElement(ns + "url", new XElement(ns + "loc", origin + "/blog")));
        foreach (var post in posts)
            root.Add(new XElement(ns + "url", new XElement(ns + "loc", origin + "/blog/" + post.Slug),
                new XElement(ns + "lastmod", post.UpdatedAt.ToString("yyyy-MM-dd"))));
        return Content(new XDocument(root).ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var normalized = SeoSlugHelper.Generate(slug);
        var post = await db.BlogPosts.AsNoTracking().Include(x => x.BlogCategory).Published(DateTime.UtcNow).SingleOrDefaultAsync(x => x.Slug == normalized);
        if (post == null) return NotFound();
        if (slug != post.Slug) return RedirectPermanent("/blog/" + post.Slug);
        ViewData["Title"] = post.SeoTitle ?? post.Title;
        ViewData["SeoDescription"] = post.SeoDescription ?? BlogContent.Excerpt(post);
        ViewData["CanonicalPath"] = "/blog/" + post.Slug;
        ViewData["OgImage"] = "https://www.detaycicek.com" + BlogContent.CoverUrl(post.CoverImage);
        var related = await db.BlogPosts.AsNoTracking().Include(x => x.BlogCategory).Published(DateTime.UtcNow).Where(x => x.Id != post.Id)
            .OrderByDescending(x => x.BlogCategoryId == post.BlogCategoryId).ThenByDescending(x => x.PublishedAt).Take(3).ToListAsync();
        return View(new BlogDetailViewModel { Post = post, RelatedPosts = related, Settings = await db.BlogSettings.AsNoTracking().FirstOrDefaultAsync() ?? new() });
    }
}
