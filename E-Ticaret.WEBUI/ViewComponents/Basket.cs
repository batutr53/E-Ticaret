using Azure;
using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.ViewComponents
{
    public class Basket : ViewComponent
    {
        private readonly IService<Product> _serviceProduct;
        private readonly IService<Cart> _serviceCart;

        public Basket(IService<Cart> serviceCart, IService<Product> serviceProduct)
        {
            _serviceCart = serviceCart;
            _serviceProduct = serviceProduct;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewBag.CartCount = 0;
            var cart = await _serviceCart.GetQueryable()
           .Include(x => x.CartItems)
           .ThenInclude(x => x.Product)
           .ThenInclude(x=>x.ProductImages)
           .FirstOrDefaultAsync(x => x.CustomerId == Request.Cookies["customerId"]);
            if (cart != null) ViewBag.CartCount = cart.CartItems.Count(); return View();
        }
    }
}
