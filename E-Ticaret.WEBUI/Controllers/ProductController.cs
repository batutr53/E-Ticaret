using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Helpers;
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

        public async Task<IActionResult> Index(string q = "", string sort = "default")
        {
            var allowedSorts = new[] { "default", "price-asc", "price-desc" };
            if (!allowedSorts.Contains(sort))
                sort = "default";

            var result = await _productService.GetFilteredProductsAsync(q, sort);
            ViewBag.SearchQuery = q;
            ViewBag.Sort = sort;
            return View(result);
        }

        public async Task<IActionResult> Detail(int productCode, string? slug = null)
        {
            var product = await _productService.GetProductDetailAsync(productCode);

            if (product == null)
            {
                return View(); 
            }

            var canonicalSlug = SeoSlugHelper.Generate(product.Name);
            if (!string.Equals(slug, canonicalSlug, StringComparison.Ordinal))
            {
                return Redirect($"/urun/{product.ProductCode}/{canonicalSlug}");
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(product);

            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
              .OrderByDescending(x => x.Id) 
              .Take(20)
              .ToList()
            };

            return View(model);
        }
    }
}
