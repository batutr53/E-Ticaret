using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Areas.Admin.Models
{
    public class FooterAdminViewModel
    {
        public List<FooterSection> Sections { get; set; }
        public List<FooterContact> Contacts { get; set; }
        public List<FooterMobileMenu> MobileMenus { get; set; }
    }

}
