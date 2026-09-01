using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SlidersController : Controller
    {
        private readonly DatabaseContext _context;

        public SlidersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Sliders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders
                .OrderBy(x => x.DisplayType)
                .ThenBy(x => x.OrderNo)
                .ToListAsync());
        }

        // GET: Admin/Sliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Admin/Sliders/Create
        public IActionResult Create(SliderDisplayType displayType = SliderDisplayType.Desktop)
        {
            return View(new Slider { DisplayType = displayType });
        }

        // POST: Admin/Sliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider, IFormFile? Image)
        {
            if (Image == null)
                ModelState.AddModelError(nameof(Slider.Image), "Slider görseli zorunludur.");

            if (ModelState.IsValid)
            {
                slider.Image = await FileHelper.FileLoaderAsync(Image!);
                if (string.IsNullOrWhiteSpace(slider.Image))
                {
                    ModelState.AddModelError(nameof(Slider.Image), "Geçerli bir görsel yükleyin.");
                    return View(slider);
                }
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Admin/Sliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Admin/Sliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider slider, IFormFile? Image, bool cbRemoveImage = false)
        {
            if (id != slider.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var current = await _context.Sliders.FindAsync(id);
                    if (current == null)
                        return NotFound();

                    current.Title = slider.Title;
                    current.Description = slider.Description;
                    current.Link = slider.Link;
                    current.IsActive = slider.IsActive;
                    current.DisplayType = slider.DisplayType;
                    current.OrderNo = slider.OrderNo;

                    if (cbRemoveImage && !string.IsNullOrWhiteSpace(current.Image))
                    {
                        FileHelper.FileRemover(current.Image);
                        current.Image = null;
                    }
                    if (Image is not null)
                    {
                        var uploaded = await FileHelper.FileLoaderAsync(Image);
                        if (string.IsNullOrWhiteSpace(uploaded))
                        {
                            ModelState.AddModelError(nameof(Slider.Image), "Geçerli bir görsel yükleyin.");
                            return View(slider);
                        }
                        if (!string.IsNullOrWhiteSpace(current.Image))
                            FileHelper.FileRemover(current.Image);
                        current.Image = uploaded;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.Id))
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
            return View(slider);
        }

        // GET: Admin/Sliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Admin/Sliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                if (!string.IsNullOrEmpty(slider.Image))
                {
                    FileHelper.FileRemover(slider.Image);
                }
                _context.Sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
    }
}
