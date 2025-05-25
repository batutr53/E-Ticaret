using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var result = await _productService.GetFilteredProductsAsync(q);
            return View(result);
        }

        public async Task<IActionResult> Detail(int productCode)
        {
            var product = await _productService.GetProductDetailAsync(productCode);

            if (product == null)
            {
                return View(); 
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(product);

            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
              .OrderByDescending(x => x.Id) // İsteğe bağlı sıralama
              .Take(20)
              .ToList()
            };

            return View(model);
        }
    }
}
