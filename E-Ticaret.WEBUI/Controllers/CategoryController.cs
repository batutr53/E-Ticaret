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
                if (id <= 0 || page < 1 || pageSize < 1)
                {
                    return BadRequest();
                }

                var category = await _categoryService.GetQueryable()
                    .Include(c => c.ProductCategories)
                        .ThenInclude(pc => pc.Product)
                            .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound();
                }

                // SEO URL doğrulama ve yönlendirme
                var expectedSeoUrl = category.Name.ToUrlFriendly();
                
                if (string.IsNullOrEmpty(seoUrl) || !string.Equals(seoUrl, expectedSeoUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToRoute("CategoryDetail", new { 
                        id, 
                        seoUrl = expectedSeoUrl,
                        page = page > 1 ? page : (int?)null 
                    });
                }

                // SEO bilgilerini ayarla
                ViewBag.PageTitle = $"{category.Name} Kategorisi";
                ViewBag.MetaDescription = $"{category.Name} kategorisindeki ürünlerimizi inceleyin. En kaliteli ürünler uygun fiyatlarla sizleri bekliyor.";
                ViewBag.CanonicalUrl = Url.RouteUrl("CategoryDetail", new { 
                    id, 
                    seoUrl = expectedSeoUrl,
                    page = page > 1 ? page : (int?)null 
                }, Request.Scheme);

                // Sayfalama için ürünleri al
                var products = category.ProductCategories
                    .Select(pc => pc.Product)
                    .Where(p => p != null && p.IsActive)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToList();

                var totalItems = products.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                
                // Geçersiz sayfa numarası kontrolü
                if (page > totalPages && totalPages > 0)
                {
                    return RedirectToRoute("CategoryDetail", new { 
                        id, 
                        seoUrl = expectedSeoUrl,
                        page = totalPages 
                    });
                }

                var pagedProducts = products
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Sayfalama bilgileri
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.CategoryId = id;
                ViewBag.CategoryName = category.Name;
                ViewBag.CategorySeoUrl = expectedSeoUrl;

                // Breadcrumb için
                ViewBag.Breadcrumb = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Ana Sayfa", Url = Url.Action("Index", "Home") },
                    new BreadcrumbItem { Text = category.Name, IsActive = true }
                };

                category.ProductCategories = pagedProducts
                    .Select(p => new E_Ticaret.Core.Entities.ProductCategory { Product = p })
                    .ToList();

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori sayfası yüklenirken bir hata oluştu - ID: {CategoryId}", id);
                throw;
            }
        }


    }
}
