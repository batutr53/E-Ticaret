using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class ProductFilter : ViewComponent
    {
        private readonly IService<Product> _productService;

        public ProductFilter(IService<Product> productService)
        {
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId)
        {
            var products = await _productService.GetQueryable()
                .Where(p => p.IsActive && p.ProductCategories.Any(pc => pc.CategoryId == categoryId && pc.Category.IsActive))
                .Include(p => p.Brand)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();

            return View(products);
        }

    }


}
