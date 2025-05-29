using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class FooterMobileMenuService : IFooterMobileMenuService
    {
        private readonly DatabaseContext _context;
        public FooterMobileMenuService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<FooterMobileMenu>> GetActiveMobileMenusAsync()
        {
            return await _context.FooterMobileMenus
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();
        }
    }

}
