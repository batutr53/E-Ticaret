using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class FooterService : IFooterService
    {
        private readonly DatabaseContext _context;

        public FooterService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<FooterSection>> GetFooterSectionsAsync()
        {
            return await _context.FooterSections
                .Include(s => s.Links.Where(l => l.IsActive)) 
                .Where(s => s.IsActive)
                .OrderBy(s => s.OrderNo)
                .ToListAsync();
        }

        public async Task<List<FooterLink>> GetFooterLinksBySectionIdAsync(int sectionId)
        {
            return await _context.FooterLinks
                .Where(l => l.SectionId == sectionId && l.IsActive)
                .OrderBy(l => l.OrderNo)
                .ToListAsync();
        }
    }

}
