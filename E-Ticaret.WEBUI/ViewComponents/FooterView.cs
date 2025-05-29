using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class FooterView : ViewComponent
    {
        private readonly IFooterService _footerService;
        private readonly IFooterContactService _contactService;
        private readonly IFooterMobileMenuService _mobileMenuService;

        public FooterView(
            IFooterService footerService,
            IFooterContactService contactService,
            IFooterMobileMenuService mobileMenuService)
        {
            _footerService = footerService;
            _contactService = contactService;
            _mobileMenuService = mobileMenuService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var vm = new FooterViewModel
                {
                    Sections = await _footerService.GetFooterSectionsAsync(),
                    Contacts = await _contactService.GetActiveContactsAsync(),
                    MobileMenus = await _mobileMenuService.GetActiveMobileMenusAsync()
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                return Content("Hata: " + ex.Message);
            }
        }
    }


}
