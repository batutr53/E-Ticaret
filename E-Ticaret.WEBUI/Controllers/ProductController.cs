using E_Ticaret.Data;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly DatabaseContext _context;

        public ProductController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string q = "")
        {
            var result = await _context.Products
                .Where(x => x.IsActive &&
                       (EF.Functions.ILike(x.Name, $"%{q}%") ||
                        EF.Functions.ILike(x.ProductCode.ToString(), $"%{q}%")))
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .ToListAsync();

            return View(result);
        }


        public async Task<IActionResult> Detail(int productCode)
        {
            var product = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive);
            if (product == null)
            {
                return View();
            }

            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = await _context.Products
                    .Where(x => x.CategoryId == product.CategoryId && x.Id != product.Id && x.IsActive)
                    .ToListAsync()
            };
            return View(model);
        }
    }
}
