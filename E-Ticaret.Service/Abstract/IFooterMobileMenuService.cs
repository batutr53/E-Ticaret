using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface IFooterMobileMenuService
    {
        Task<List<FooterMobileMenu>> GetActiveMobileMenusAsync();
    }

}
