using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IService<Product> _productService;

        public ProductController(IService<Product> productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var result = await _productService.GetQueryable()
                .Where(x => x.IsActive &&
                    (EF.Functions.ILike(x.Name, $"%{q}%") ||
                     EF.Functions.ILike(x.ProductCode.ToString(), $"%{q}%")))
                .Include(x => x.Brand)
                .Include(p => p.ProductImages)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();

            return View(result);
        }



        public async Task<IActionResult> Detail(int productCode)
        {
            var product = await _productService.GetQueryable()
                .Include(x => x.Brand)
                .Include(p => p.ProductImages)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive);

            if (product == null)
            {
                return View(); // veya NotFound()
            }

            // İlgili kategorilerden birini baz alarak benzer ürünleri getir
            var relatedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();

            var relatedProducts = await _productService.GetQueryable()
                .Where(x => x.IsActive && x.Id != product.Id &&
                            x.ProductCategories.Any(pc => relatedCategoryIds.Contains(pc.CategoryId)))
                .Include(x => x.Brand)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();

            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
            };

            return View(model);
        }

    }
}
