using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface ISiteSettingService
    {
        Task<SiteSetting> GetAsync();
        Task SaveAsync(SiteSetting setting);
    }
}
