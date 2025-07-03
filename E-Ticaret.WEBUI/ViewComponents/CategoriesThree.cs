using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class CategoriesThree : ViewComponent
    {
        private readonly IService<Category> _categoryService;

        public CategoriesThree(IService<Category> categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllAsync(x => x.IsActive && x.IsTopMenu && x.ParentId == 4);
            return View(categories);
        }
    }


}
