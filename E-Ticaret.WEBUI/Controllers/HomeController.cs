using System.Diagnostics;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseContext _context;
        public HomeController(ILogger<HomeController> logger, DatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Sliders = await _context.Sliders.ToListAsync();

            var result = await _context.Products.Where(x=>x.IsActive && x.IsHome)
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .ToListAsync();
            return View(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }
        public async Task<IActionResult> ContactUsPost(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Contacts.Add(contact);
                    var result = await _context.SaveChangesAsync();
                    if (result > 0)
                    {
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baþarýlý', text: 'Mesajýnýz baþarýyla gönderilmiþtir.'});";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baþarýlý', text: 'Mesajýnýz gönderilemedi. Lütfen tekrar deneyin.'});";;
                        return RedirectToAction("ContactUs");
                    }

                }
                catch (Exception)
                {
                     ModelState.AddModelError("", "Mesajýnýz gönderilemedi. Lütfen tekrar deneyin.");
                    throw;
                }
            }
          

            return View(contact);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
