using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Extensions;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Controllers
{
    [Route("kategori")]
    public class CategoryController : Controller
    {
        private readonly IService<Category> _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IService<Category> categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [Route("{id:int}/{seoUrl?}", Name = "CategoryDetail")]
        [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id", "page" })]
        public async Task<IActionResult> Index(int id, string seoUrl = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Kategori sayfası isteği alındı - ID: {CategoryId}, SEO URL: {SeoUrl}, Sayfa: {Page}, Sayfa Boyutu: {PageSize}", 
                    id, seoUrl, page, pageSize);

                if (id <= 0 || page < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Geçersiz parametreler - ID: {CategoryId}, Page: {Page}, PageSize: {PageSize}", id, page, pageSize);
                    return BadRequest();
                }

                _logger.LogInformation("Kategori veritabanından çekiliyor...");
                var category = await _categoryService.GetQueryable()
                    .Include(c => c.ProductCategories)
                        .ThenInclude(pc => pc.Product)
                            .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    _logger.LogWarning("Kategori bulunamadı - ID: {CategoryId}", id);
                    return NotFound();
                }

                _logger.LogInformation("Kategori bulundu: {CategoryName}", category.Name);

                // SEO URL doğrulama ve yönlendirme
                var expectedSeoUrl = category.Name.ToUrlFriendly();
                _logger.LogInformation("Beklenen SEO URL: {ExpectedSeoUrl}, Gelen SEO URL: {SeoUrl}", expectedSeoUrl, seoUrl);
            
                if (string.IsNullOrEmpty(seoUrl) || !string.Equals(seoUrl, expectedSeoUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("SEO URL uyumsuz, yönlendiriliyor...");
                    return RedirectToRoute("CategoryDetail", new { 
                        id, 
                        seoUrl = expectedSeoUrl,
                        page = page > 1 ? page : (int?)null 
                    });
                }

                _logger.LogInformation("SEO URL doğrulandı");

                // SEO bilgilerini ayarla
                ViewBag.PageTitle = $"{category.Name} Kategorisi";
                ViewBag.MetaDescription = $"{category.Name} kategorisindeki ürünlerimizi inceleyin. En kaliteli ürünler uygun fiyatlarla sizleri bekliyor.";
                ViewBag.CanonicalUrl = Url.RouteUrl("CategoryDetail", new { 
                    id, 
                    seoUrl = expectedSeoUrl,
                    page = page > 1 ? page : (int?)null 
                }, Request.Scheme);

                _logger.LogInformation("Kategoriye ait ürünler filtreleniyor...");
                // Sayfalama için ürünleri al
                var products = category.ProductCategories
                    .Select(pc => pc.Product)
                    .Where(p => p != null && p.IsActive)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToList();

                _logger.LogInformation("Toplam {Count} ürün bulundu", products.Count);

                var totalItems = products.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                _logger.LogInformation("Toplam sayfa sayısı: {TotalPages}, Mevcut sayfa: {Page}", totalPages, page);
                
                // Geçersiz sayfa numarası kontrolü
                if (page > totalPages && totalPages > 0)
                {
                    _logger.LogWarning("Geçersiz sayfa numarası: {Page}, Toplam sayfa: {TotalPages}", page, totalPages);
                    return RedirectToRoute("CategoryDetail", new { 
                        id, 
                        seoUrl = expectedSeoUrl,
                        page = totalPages 
                    });
                }

                _logger.LogInformation("Sayfalama uygulanıyor...");
                var pagedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Sayfalama tamamlandı, {Count} ürün sayfalandı", pagedProducts.Count);

                // Sayfalama bilgileri
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.CategoryId = id;
                ViewBag.CategoryName = category.Name;
                ViewBag.CategorySeoUrl = expectedSeoUrl;

                _logger.LogInformation("Breadcrumb bilgileri ayarlanıyor...");
                // Breadcrumb için
                ViewBag.Breadcrumb = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Ana Sayfa", Url = Url.Action("Index", "Home") },
                    new BreadcrumbItem { Text = category.Name, IsActive = true }
                };

                _logger.LogInformation("Ürün kategorileri ayarlanıyor...");
                category.ProductCategories = pagedProducts
                    .Select(p => new E_Ticaret.Core.Entities.ProductCategory { Product = p })
                    .ToList();

                _logger.LogInformation("Kategori sayfası gösteriliyor");
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori sayfası yüklenirken bir hata oluştu - ID: {CategoryId}", id);
                throw; // Hata detaylarını görmek için hatayı tekrar fırlatıyoruz
            }
        }


    }
}
