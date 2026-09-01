using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class SiteSettingService : ISiteSettingService
    {
        private readonly DatabaseContext _context;

        public SiteSettingService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<SiteSetting> GetAsync()
        {
            return await _context.SiteSettings.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == SiteSetting.SingletonId)
                ?? new SiteSetting();
        }

        public async Task SaveAsync(SiteSetting setting)
        {
            setting.Id = SiteSetting.SingletonId;
            var current = await _context.SiteSettings
                .SingleOrDefaultAsync(x => x.Id == SiteSetting.SingletonId);

            if (current == null)
                _context.SiteSettings.Add(setting);
            else
            {
                current.Logo = setting.Logo;
                current.PrimaryColor = setting.PrimaryColor;
                current.AccentColor = setting.AccentColor;
            }

            await _context.SaveChangesAsync();
        }
    }
}
