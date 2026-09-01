using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MobileBannersController : Controller
    {
        private readonly DatabaseContext _context;

        public MobileBannersController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var banners = await _context.MobileBanners.OrderBy(x => x.OrderNo).ToListAsync();
            ViewBag.ActiveCount = banners.Count(x => x.IsActive);
            return View(banners);
        }

        public IActionResult Create() => View(new MobileBanner());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MobileBanner banner, IFormFile? Image)
        {
            if (Image == null)
                ModelState.AddModelError(nameof(MobileBanner.Image), "Görsel zorunludur.");
            await ValidateActiveLimitAsync(banner.IsActive);

            if (!ModelState.IsValid)
                return View(banner);

            banner.Image = await FileHelper.FileLoaderAsync(Image!);
            if (string.IsNullOrWhiteSpace(banner.Image))
            {
                ModelState.AddModelError(nameof(MobileBanner.Image), "Geçerli bir görsel yükleyin.");
                return View(banner);
            }

            _context.MobileBanners.Add(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _context.MobileBanners.FindAsync(id);
            return banner == null ? NotFound() : View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MobileBanner banner, IFormFile? Image)
        {
            if (id != banner.Id)
                return NotFound();

            var current = await _context.MobileBanners.FindAsync(id);
            if (current == null)
                return NotFound();

            await ValidateActiveLimitAsync(banner.IsActive && !current.IsActive);
            if (!ModelState.IsValid)
            {
                banner.Image = current.Image;
                return View(banner);
            }

            current.Link = banner.Link;
            current.IsActive = banner.IsActive;
            current.OrderNo = banner.OrderNo;
            if (Image != null)
            {
                var uploaded = await FileHelper.FileLoaderAsync(Image);
                if (string.IsNullOrWhiteSpace(uploaded))
                {
                    ModelState.AddModelError(nameof(MobileBanner.Image), "Geçerli bir görsel yükleyin.");
                    banner.Image = current.Image;
                    return View(banner);
                }
                if (!string.IsNullOrWhiteSpace(current.Image))
                    FileHelper.FileRemover(current.Image);
                current.Image = uploaded;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.MobileBanners.FindAsync(id);
            if (banner == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(banner.Image))
                FileHelper.FileRemover(banner.Image);
            _context.MobileBanners.Remove(banner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> ids)
        {
            var banners = await _context.MobileBanners.Where(x => ids.Contains(x.Id)).ToListAsync();
            if (banners.Count != ids.Distinct().Count())
                return BadRequest(new { success = false, message = "Geçersiz banner listesi." });

            for (var index = 0; index < ids.Count; index++)
                banners.Single(x => x.Id == ids[index]).OrderNo = index;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        private async Task ValidateActiveLimitAsync(bool activating)
        {
            if (activating && await _context.MobileBanners.CountAsync(x => x.IsActive) >= 3)
                ModelState.AddModelError(nameof(MobileBanner.IsActive), "En fazla 3 mobil banner aktif olabilir.");
        }
    }
}
