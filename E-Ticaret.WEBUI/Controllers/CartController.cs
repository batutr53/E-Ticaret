using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IService<Product> _serviceProduct;
        private readonly IService<Cart> _serviceCart;

        public CartController(IService<Product> serviceProduct, IService<Cart> serviceCart)
        {
            _serviceProduct = serviceProduct;
            _serviceCart = serviceCart;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreate();
            ViewBag.TotalPrice = 0;
            foreach (var item in cart.CartItems)
            {
                ViewBag.TotalPrice += item.Product.Price * item.Quantity;
            }
            return View(cart);
        }

        [HttpPost("AddItemToCart")]
        public async Task<IActionResult> AddItemToCart(int productId, int quantity = 1)
        {
            quantity = quantity < 0 ? -quantity : (quantity == 0 || quantity == null ? 1 : quantity);

            var cart = await GetOrCreate();

            var product = await _serviceProduct.GetQueryable().FirstOrDefaultAsync(x => x.Id == productId);
            if (product != null)
            {
                cart.AddItem(product, quantity);
                await _serviceCart.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost("DeleteItemFromCart")]
        public async Task<IActionResult> DeleteItemFromCart(int productId, int quantity)
        {
            var cart = await GetOrCreate();
            cart.DeleteItem(productId, quantity);
            var result = await _serviceProduct.SaveChangesAsync() > 0;
            return RedirectToAction("Index");
        }

        public async Task<Cart> GetOrCreate()
        {
            var cart = await _serviceCart.GetQueryable()
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.CustomerId == Request.Cookies["customerId"]);
            if (cart == null)
            {
                var customerId = Guid.NewGuid().ToString();

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    IsEssential = true
                };

                Response.Cookies.Append("customerId", customerId, cookieOptions);


                cart = new Cart
                {
                    CustomerId = customerId,
                    CartItems = new List<CartItem>()
                };
                await _serviceCart.AddAsync(cart);
                await _serviceCart.SaveChangesAsync();
            }
            return cart;
        }
    }
}
