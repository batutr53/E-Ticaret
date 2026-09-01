using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContentPagesController : Controller
    {
        private readonly DatabaseContext _context;

        public ContentPagesController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ContentPages
                .Where(x => ContentPageKeys.All.Contains(x.Key))
                .OrderBy(x => x.Id)
                .ToListAsync());
        }

        public async Task<IActionResult> Edit(string key)
        {
            if (!ContentPageKeys.All.Contains(key))
                return NotFound();
            var page = await _context.ContentPages.SingleOrDefaultAsync(x => x.Key == key);
            return page == null ? NotFound() : View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string key, ContentPage model)
        {
            if (!ContentPageKeys.All.Contains(key) || key != model.Key)
                return NotFound();

            var page = await _context.ContentPages.SingleOrDefaultAsync(x => x.Key == key);
            if (page == null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(model);

            page.Title = model.Title.Trim();
            page.BodyHtml = model.BodyHtml;
            await _context.SaveChangesAsync();
            TempData["ContentPageMessage"] = "Sayfa kaydedildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
