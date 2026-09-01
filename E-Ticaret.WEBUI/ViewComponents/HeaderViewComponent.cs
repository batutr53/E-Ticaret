using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly IService<Category> _categoryService;
        private readonly ISiteSettingService _siteSettingService;

        public HeaderViewComponent(
            IService<Category> categoryService,
            ISiteSettingService siteSettingService)
        {
            _categoryService = categoryService;
            _siteSettingService = siteSettingService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categoryCounts = await _categoryService.GetQueryable()
                .Where(x => x.IsActive && x.IsTopMenu &&
                    (x.ParentId == 1 || x.ParentId == 54 || x.ParentId == 4))
                .GroupBy(x => x.ParentId)
                .Select(group => new { ParentId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(x => x.ParentId, x => x.Count);

            return View(new HeaderViewModel
            {
                SiteSetting = await _siteSettingService.GetAsync(),
                CategoriesCount = categoryCounts.GetValueOrDefault(1),
                DeliveryTypesCount = categoryCounts.GetValueOrDefault(54),
                ArtificialFlowersCount = categoryCounts.GetValueOrDefault(4)
            });
        }
    }
}
