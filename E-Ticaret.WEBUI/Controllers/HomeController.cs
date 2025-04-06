using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IService<Product> _productService;
        private readonly IService<Slider> _sliderService;
        private readonly IService<Contact> _contactService;

        public HomeController(IService<Slider> sliderService, IService<Product> productService, IService<Contact> contactService)
        {
            _sliderService = sliderService;
            _productService = productService;
            _contactService = contactService;
        }



        public async Task<IActionResult> Index()
        {
            ViewBag.Sliders = await _sliderService.GetAllAsync();

            var result = await _productService.GetQueryable().Where(x => x.IsActive && x.IsHome)
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
                    _contactService.Add(contact);
                    var result = await _contactService.SaveChangesAsync();
                    if (result > 0)
                    {
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baþarýlý', text: 'Mesajýnýz baþarýyla gönderilmiþtir.'});";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baþarýlý', text: 'Mesajýnýz gönderilemedi. Lütfen tekrar deneyin.'});"; ;
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

    }
}
