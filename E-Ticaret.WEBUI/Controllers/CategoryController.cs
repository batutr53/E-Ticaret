using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
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
        public async Task<IActionResult> Index(int id, int page = 1, int pageSize = 20)

        {
            var category = await _categoryService.GetQueryable()
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var products = category.ProductCategories
                .Select(pc => pc.Product)
                .OrderByDescending(p => p.Id)
                .ToList();

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)products.Count / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = id;

            category.ProductCategories = pagedProducts
                .Select(p => new E_Ticaret.Core.Entities.ProductCategory { Product = p })
                .ToList();

            return View(category);
        }


    }
}
