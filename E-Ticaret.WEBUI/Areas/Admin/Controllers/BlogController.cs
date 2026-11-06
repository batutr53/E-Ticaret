using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Models;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class BlogController(DatabaseContext db, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(string? q, string? status, int page = 1)
    {
        var query = db.BlogPosts.AsNoTracking().Include(x => x.BlogCategory).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Title.ToLower().Contains(q.Trim().ToLower()));
        if (status == "draft") query = query.Where(x => !x.IsPublished);
        if (status == "published") query = query.Where(x => x.IsPublished && x.PublishedAt <= DateTime.UtcNow);
        if (status == "scheduled") query = query.Where(x => x.IsPublished && x.PublishedAt > DateTime.UtcNow);
        var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / 20d)));
        ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling(total / 20d);
        ViewBag.Search = q; ViewBag.Status = status; ViewBag.Total = total;
        return View(await query.OrderByDescending(x => x.UpdatedAt).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return View(await Prepare(new BlogEditViewModel()));
        var post = await db.BlogPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (post == null) return NotFound();
        return View(await Prepare(new BlogEditViewModel
        {
            Id = post.Id, Title = post.Title, Slug = post.Slug, Excerpt = post.Excerpt, BodyHtml = post.BodyHtml,
            ExistingCover = post.CoverImage, CoverAlt = post.CoverAlt, Author = post.Author,
            SeoTitle = post.SeoTitle, SeoDescription = post.SeoDescription, BlogCategoryId = post.BlogCategoryId,
            IsPublished = post.IsPublished, PublishAtLocal = post.PublishedAt.HasValue ? BlogContent.LocalDate(post.PublishedAt.Value) : null,
            SlugLocked = post.PublishedAt.HasValue, Version = post.UpdatedAt.Ticks
        }));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Save(BlogEditViewModel model, string intent, CancellationToken cancellationToken)
    {
        var post = model.Id == 0 ? new BlogPost() : await db.BlogPosts.SingleOrDefaultAsync(x => x.Id == model.Id, cancellationToken);
        if (post == null) return NotFound();
        model.ExistingCover = post.CoverImage;
        model.SlugLocked = post.PublishedAt.HasValue;
        if (model.Id != 0 && post.UpdatedAt.Ticks != model.Version)
            ModelState.AddModelError("", "Bu yazı başka bir ekranda değiştirildi. Sayfayı yenileyip tekrar düzenleyin.");
        var slug = SeoSlugHelper.Generate(string.IsNullOrWhiteSpace(model.Slug) ? model.Title : model.Slug);
        if (post.PublishedAt.HasValue && post.Slug != slug)
            ModelState.AddModelError(nameof(model.Slug), "Yayınlanmış yazının bağlantısı SEO için korunur.");
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 220)
            ModelState.AddModelError(nameof(model.Slug), "Geçerli bir bağlantı adı girin.");
        else if (await db.BlogPosts.AnyAsync(x => x.Slug == slug && x.Id != model.Id, cancellationToken))
            ModelState.AddModelError(nameof(model.Slug), "Bu bağlantı başka bir yazıda kullanılıyor.");
        if (model.BlogCategoryId.HasValue && !await db.BlogCategories.AnyAsync(x => x.Id == model.BlogCategoryId, cancellationToken))
            ModelState.AddModelError(nameof(model.BlogCategoryId), "Kategori bulunamadı.");
        if (intent is not ("draft" or "publish")) ModelState.AddModelError("", "Taslak kaydet veya Yayınla seçeneğini kullanın.");
        var body = RichTextHelper.Normalize(model.BodyHtml);
        if (intent == "publish")
        {
            if (BlogContent.PlainText(body).Length < 20) ModelState.AddModelError(nameof(model.BodyHtml), "Yayınlamak için yazı içeriğini girin (en az 20 karakter).");
            if (model.Cover == null && string.IsNullOrWhiteSpace(post.CoverImage)) ModelState.AddModelError(nameof(model.Cover), "Yayınlamak için kapak görseli yükleyin.");
            if (model.BlogCategoryId.HasValue && !await db.BlogCategories.AnyAsync(x => x.Id == model.BlogCategoryId && x.IsActive, cancellationToken))
                ModelState.AddModelError(nameof(model.BlogCategoryId), "Yayınlamak için aktif kategori seçin.");
        }
        if (!ModelState.IsValid) return View("Edit", await Prepare(model));
        if (model.Cover != null)
        {
            try { post.CoverImage = await new BlogImageStorage(environment).SaveAsync(model.Cover, cancellationToken); }
            catch (Exception exception) when (exception is InvalidDataException or UnknownImageFormatException or InvalidImageContentException)
            {
                ModelState.AddModelError(nameof(model.Cover), "Görsel yüklenemedi. Geçerli JPG, PNG veya WebP, en fazla 10 MB / 40 megapiksel yükleyin.");
                return View("Edit", await Prepare(model));
            }
        }
        post.Title = model.Title.Trim(); post.Slug = slug;
        post.Excerpt = model.Excerpt?.Trim() ?? ""; post.BodyHtml = body;
        post.CoverAlt = string.IsNullOrWhiteSpace(model.CoverAlt) ? post.Title : model.CoverAlt.Trim();
        post.Author = string.IsNullOrWhiteSpace(model.Author) ? "Detay Çiçek Ekibi" : model.Author.Trim();
        post.SeoTitle = string.IsNullOrWhiteSpace(model.SeoTitle) ? null : model.SeoTitle.Trim();
        post.SeoDescription = string.IsNullOrWhiteSpace(model.SeoDescription) ? null : model.SeoDescription.Trim();
        post.BlogCategoryId = model.BlogCategoryId; post.IsPublished = intent == "publish";
        if (model.PublishAtLocal.HasValue) post.PublishedAt = BlogContent.UtcDate(model.PublishAtLocal.Value);
        if (post.IsPublished && !model.PublishAtLocal.HasValue && (!post.PublishedAt.HasValue || post.PublishedAt > DateTime.UtcNow))
            post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        if (post.Id == 0) db.BlogPosts.Add(post);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "Yazı başka bir ekranda değişti. Kaydedilmedi; sayfayı yenileyin.");
            return View("Edit", await Prepare(model));
        }
        catch (DbUpdateException exception) when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            ModelState.AddModelError(nameof(model.Slug), "Bu bağlantı başka bir yazıda kullanılıyor.");
            return View("Edit", await Prepare(model));
        }
        TempData["BlogMessage"] = !post.IsPublished ? "Taslak kaydedildi. Yalnızca yöneticiler önizleyebilir."
            : post.PublishedAt > DateTime.UtcNow ? "Yazı planlandı. Belirlediğiniz tarihte görünür olacak." : "Yazı yayınlandı.";
        return RedirectToAction(nameof(Edit), new { id = post.Id });
    }

    public async Task<IActionResult> Preview(int id)
    {
        var post = await db.BlogPosts.AsNoTracking().Include(x => x.BlogCategory).SingleOrDefaultAsync(x => x.Id == id);
        if (post == null) return NotFound();
        Response.Headers.CacheControl = "no-store, private";
        Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        ViewData["Title"] = post.Title; ViewData["Robots"] = "noindex,nofollow";
        return View("~/Views/Blog/Detail.cshtml", new BlogDetailViewModel
        {
            Post = post, IsPreview = true, Settings = await db.BlogSettings.AsNoTracking().FirstOrDefaultAsync() ?? new()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        await db.BlogPosts.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsPublished, false).SetProperty(x => x.UpdatedAt, DateTime.UtcNow));
        TempData["BlogMessage"] = "Yazı yayından kaldırıldı, taslak olarak korunuyor.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null) return BadRequest(new { error = "Görsel seçin." });
        try
        {
            var name = await new BlogImageStorage(environment).SaveAsync(file, cancellationToken);
            return Json(new { location = BlogContent.CoverUrl(name) });
        }
        catch (Exception exception) when (exception is InvalidDataException or UnknownImageFormatException or InvalidImageContentException)
        {
            return BadRequest(new { error = "Geçerli bir JPG, PNG veya WebP yükleyin. En fazla 10 MB / 40 megapiksel." });
        }
    }

    public async Task<IActionResult> Categories() => View(await db.BlogCategories.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(int id, string? name, int sortOrder, bool isActive)
    {
        var category = id == 0 ? new BlogCategory() : await db.BlogCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category == null) return NotFound();
        var slug = SeoSlugHelper.Generate(name);
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100 || string.IsNullOrEmpty(slug) || await db.BlogCategories.AnyAsync(x => x.Slug == slug && x.Id != id))
        {
            TempData["BlogMessage"] = "Kategori adı 1–100 karakter olmalı ve başka bir kategoriyle aynı olmamalı.";
            return RedirectToAction(nameof(Categories));
        }
        category.Name = name.Trim(); category.Slug = slug; category.SortOrder = sortOrder; category.IsActive = isActive;
        if (id == 0) db.BlogCategories.Add(category);
        await db.SaveChangesAsync();
        TempData["BlogMessage"] = "Kategori kaydedildi.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> Settings() => View(await db.BlogSettings.AsNoTracking().FirstOrDefaultAsync() ?? new());

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Settings(BlogSetting model, IFormFile? banner, CancellationToken cancellationToken)
    {
        var setting = await db.BlogSettings.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken) ?? new();
        model.BannerImage = setting.BannerImage;
        if (!ModelState.IsValid) return View(model);
        if (banner != null)
        {
            try { setting.BannerImage = await new BlogImageStorage(environment).SaveAsync(banner, cancellationToken); }
            catch (Exception exception) when (exception is InvalidDataException or UnknownImageFormatException or InvalidImageContentException)
            {
                ModelState.AddModelError("banner", "Geçerli bir görsel yükleyin (en fazla 10 MB / 40 megapiksel).");
                return View(model);
            }
        }
        setting.Title = model.Title.Trim(); setting.Description = model.Description?.Trim() ?? "";
        if (!await db.BlogSettings.AnyAsync(cancellationToken)) db.BlogSettings.Add(setting);
        await db.SaveChangesAsync(cancellationToken);
        TempData["BlogMessage"] = "Blog görünümü güncellendi.";
        return RedirectToAction(nameof(Settings));
    }

    private async Task<BlogEditViewModel> Prepare(BlogEditViewModel model)
    {
        model.Categories = await db.BlogCategories.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        return model;
    }
}
