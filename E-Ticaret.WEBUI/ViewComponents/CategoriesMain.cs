using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class CategoriesMain : ViewComponent
    {
        private readonly IService<Category> _categoryService;

        public CategoriesMain(IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllAsync(x => x.IsActive && x.IsTopMenu && x.ParentId == 0 && x.Id != 1);
            return View(categories);
        }
    }


}
