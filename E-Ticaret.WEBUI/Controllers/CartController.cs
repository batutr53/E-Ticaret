using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using Microsoft.AspNetCore.Mvc;
using E_Ticaret.WEBUI.ExtensionMethods;
using E_Ticaret.WEBUI.Models;

namespace E_Ticaret.WEBUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _serviceProduct;

        public CartController(IService<Product> serviceProduct)
        {
            _serviceProduct = serviceProduct;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            var model = new CartViewModel
            {
                CartLines = cart.CartLines,
                TotalPrice = cart.TotalPrice()
            };
            return View(model);
        }

        public IActionResult Add(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddToProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Update(int ProductId, int quantity = 1)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.UpdateProduct(product, quantity);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int ProductId)
        {
            var product = _serviceProduct.Find(ProductId);
            if (product != null)
            {
                var cart = GetCart();
                cart.RemoveProduct(product);
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        private CartService GetCart()
        {
            return HttpContext.Session.GetJson<CartService>("Cart") ?? new CartService();
        }


    }
}
