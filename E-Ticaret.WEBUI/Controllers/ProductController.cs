using E_Ticaret.Data;
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

        public async Task<IActionResult> Index()
        {
            var result = await _context.Products.Where(x => x.IsActive)
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
            return View(product);
        }
    }
}
