using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface IFooterService
    {
        Task<List<FooterSection>> GetFooterSectionsAsync();
        Task<List<FooterLink>> GetFooterLinksBySectionIdAsync(int sectionId);
    }

}
