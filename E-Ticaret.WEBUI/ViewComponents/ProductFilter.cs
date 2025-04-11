using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

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
            var products = await _productService.GetAllAsync(x => x.Category!.IsActive && x.CategoryId == categoryId);
            return View(products);
        }
    }


}
