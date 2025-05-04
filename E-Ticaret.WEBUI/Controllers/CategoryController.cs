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

        public async Task<IActionResult> Index(int id)
        {
            var category = await _categoryService.GetQueryable()
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                    .ThenInclude(x=>x.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(category);
        }

    }
}
