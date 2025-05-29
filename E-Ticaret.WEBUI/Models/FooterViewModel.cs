using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class FooterViewModel
    {
        public List<FooterSection> Sections { get; set; }
        public List<FooterContact> Contacts { get; set; }
        public List<FooterMobileMenu> MobileMenus { get; set; }
    }

}
