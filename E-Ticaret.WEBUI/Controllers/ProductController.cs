using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Extensions;
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

        [Route("urun/{id:int}/{seoUrl?}", Name = "ProductDetail")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        public async Task<IActionResult> Detail(int id, string seoUrl)
        {
            var product = await _productService.GetProductDetailAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // SEO URL doğrulama ve yönlendirme
            var expectedSeoUrl = product.Name.ToUrlFriendly();
            
            if (string.IsNullOrEmpty(seoUrl) || !string.Equals(seoUrl, expectedSeoUrl, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToRoute("ProductDetail", new { id, seoUrl = expectedSeoUrl });
            }

            // ViewBag ile SEO bilgilerini gönder
            ViewBag.PageTitle = product.Name;
            ViewBag.MetaDescription = product.Description?.Length > 160 ? 
                product.Description.Substring(0, 157) + "..." : product.Description;
            ViewBag.CanonicalUrl = Url.RouteUrl("ProductDetail", new { id, seoUrl = expectedSeoUrl }, Request.Scheme);
            ViewBag.OgImage = product.ProductImages?.FirstOrDefault()?.ImageUrl;

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
