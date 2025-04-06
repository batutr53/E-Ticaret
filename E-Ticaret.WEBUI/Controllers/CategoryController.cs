using E_Ticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DatabaseContext _context;

        public CategoryController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            return View(category);
        }
    }
}
