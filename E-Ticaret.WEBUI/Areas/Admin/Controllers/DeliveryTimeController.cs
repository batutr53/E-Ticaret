using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DeliveryTimeController : Controller
    {
        private readonly DatabaseContext _context;

        public DeliveryTimeController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ranges = await _context.DeliveryTimeRanges.ToListAsync();
            return View(ranges);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(DeliveryTimeRange model)
        {
            _context.DeliveryTimeRanges.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var range = await _context.DeliveryTimeRanges.FindAsync(id);
            return View(range);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DeliveryTimeRange model)
        {
            _context.DeliveryTimeRanges.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var range = await _context.DeliveryTimeRanges.FindAsync(id);
            if (range != null)
            {
                _context.DeliveryTimeRanges.Remove(range);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
