using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class HomeIndexViewModel
    {
        public IReadOnlyList<Product> Products { get; init; } = [];
        public IReadOnlyList<Slider> DesktopSliders { get; init; } = [];
        public IReadOnlyList<Slider> MobileSliders { get; init; } = [];
        public IReadOnlyList<MobileBanner> MobileBanners { get; init; } = [];
    }
}
