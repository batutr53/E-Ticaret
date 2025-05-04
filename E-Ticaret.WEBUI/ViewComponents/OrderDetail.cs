using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class OrderDetail : ViewComponent
    {
        private readonly IService<Cart> _serviceCart;

        public OrderDetail(IService<Cart> serviceCart)
        {
            _serviceCart = serviceCart;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _serviceCart.GetQueryable()
           .Include(x => x.CartItems)
           .ThenInclude(x => x.Product)
           .ThenInclude(x=>x.ProductImages)
           .FirstOrDefaultAsync(x => x.CustomerId == Request.Cookies["customerId"]);
            if (cart != null) return View(cart);
            return View();
        }
    }
}
