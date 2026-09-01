using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IService<Category> _categoryService;

        public CategoryController(IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            int id,
            string? slug = null,
            int page = 1,
            int pageSize = 20,
            string sort = "default")

        {
            var allowedSorts = new[] { "default", "price-asc", "price-desc" };
            if (!allowedSorts.Contains(sort))
                sort = "default";
            page = Math.Max(page, 1);

            var category = await _categoryService.GetQueryable()
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var canonicalSlug = SeoSlugHelper.Generate(category.Name);
            if (!string.Equals(slug, canonicalSlug, StringComparison.Ordinal))
            {
                return RedirectToAction(nameof(Index), new { id = category.Id, slug = canonicalSlug, page, pageSize, sort });
            }

            var productQuery = category.ProductCategories
                .OrderBy(pc => pc.OrderNo)
                .Select(pc => pc.Product);

            var products = sort switch
            {
                "price-asc" => productQuery.OrderBy(p => p.Price).ToList(),
                "price-desc" => productQuery.OrderByDescending(p => p.Price).ToList(),
                _ => productQuery.ToList()
            };

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)products.Count / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = id;
            ViewBag.Sort = sort;

            category.ProductCategories = pagedProducts
                .Select(p => new E_Ticaret.Core.Entities.ProductCategory { Product = p })
                .ToList();

            return View(category);
        }


    }
}
