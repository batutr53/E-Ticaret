using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IService<Product> _productService;
        private readonly IService<Slider> _sliderService;
        private readonly IService<Contact> _contactService;
        private readonly DatabaseContext _context;

        public HomeController(IService<Slider> sliderService, IService<Product> productService, IService<Contact> contactService, DatabaseContext context)
        {
            _sliderService = sliderService;
            _productService = productService;
            _contactService = contactService;
            _context = context;
        }



        public async Task<IActionResult> Index()
        {
            var desktopSliders = await _sliderService.GetQueryable()
                .Where(x => x.IsActive && x.DisplayType == SliderDisplayType.Desktop)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();
            var mobileSliders = await _sliderService.GetQueryable()
                .Where(x => x.IsActive && x.DisplayType == SliderDisplayType.Mobile)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();
            var mobileBanners = await _context.MobileBanners
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderNo)
                .Take(3)
                .ToListAsync();

            var result = await _productService.GetQueryable()
                .Where(x => x.IsActive && x.IsHome)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(x => x.Brand)
                .Include(x => x.ProductImages)
                .OrderBy(x => x.OrderNo) 
                .ToListAsync();

            return View(new HomeIndexViewModel
            {
                Products = result,
                DesktopSliders = desktopSliders,
                MobileSliders = mobileSliders,
                MobileBanners = mobileBanners
            });
        }

        public async Task<IActionResult> About()
        {
            return View(await GetContentPageAsync(ContentPageKeys.About));
        }

        public IActionResult ContactUs()
        {
            return View();
        }
        public async Task<IActionResult> DelivertWarranty()
        {
            return View(await GetContentPageAsync(ContentPageKeys.DeliveryWarranty));
        }
        public async Task<IActionResult> PrivacySecurity()
        {
            return View(await GetContentPageAsync(ContentPageKeys.PrivacySecurity));
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
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baùarùlù', text: 'Mesajùnùz baùarùyla gùnderilmiùtir.'});";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Message"] = "Swal.fire({icon: 'success', title: 'Baùarùlù', text: 'Mesajùnùz gùnderilemedi. Lùtfen tekrar deneyin.'});"; ;
                        return RedirectToAction("ContactUs");
                    }

                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Mesajùnùz gùnderilemedi. Lùtfen tekrar deneyin.");
                    throw;
                }
            }


            return View(contact);
        }

        private async Task<ContentPage> GetContentPageAsync(string key)
        {
            return await _context.ContentPages.AsNoTracking().SingleAsync(x => x.Key == key);
        }

    }
}
