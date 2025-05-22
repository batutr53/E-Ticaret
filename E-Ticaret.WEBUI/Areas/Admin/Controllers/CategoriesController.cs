using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IService<Category> _service;
        public CategoriesController(DatabaseContext context, IService<Category> service)
        {
            _context = context;
            _service = service;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _service
                .GetAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories,"Id","Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category,IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                category.Image = await FileHelper.FileLoaderAsync(Image);
                await _service.AddAsync(category);
                await _service.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
          
            var category = await _service.FindAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? Image, bool cbRemoveImage = false)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.CreatedDate.Kind != DateTimeKind.Utc)
                        category.CreatedDate = DateTime.SpecifyKind(category.CreatedDate, DateTimeKind.Utc);

                    if (cbRemoveImage)
                        category.Image = string.Empty;
                      category.Image = await FileHelper.FileLoaderAsync(Image) ?? "";
                    _service.Update(category);
                    await _service.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _service
                .GetAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _service.FindAsync(id);
            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.Image))
                {
                    FileHelper.FileRemover(category.Image);
                }
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _service.Get(x => x.Id == id) != null;
        }

    }
}
