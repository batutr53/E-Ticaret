using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class HeaderViewModel
    {
        public required SiteSetting SiteSetting { get; init; }
        public int CategoriesCount { get; init; }
        public int DeliveryTypesCount { get; init; }
        public int ArtificialFlowersCount { get; init; }
    }
}
