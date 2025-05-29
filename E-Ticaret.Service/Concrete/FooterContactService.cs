using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class FooterContactService : IFooterContactService
    {
        private readonly DatabaseContext _context;
        public FooterContactService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<FooterContact>> GetActiveContactsAsync()
        {
            return await _context.FooterContacts
                .Where(x => x.IsActive)
                .OrderBy(x => x.OrderNo)
                .ToListAsync();
        }
    }

}
