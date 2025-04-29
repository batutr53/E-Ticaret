using Azure.Core;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class CartService : ICartService
    {
        private readonly DatabaseContext _context;

        public CartService(DatabaseContext context)
        {
            _context = context;
        }

        public List<CartItem> CartLines = new();
        public void AddToProduct(Product product, int quantity)
        {
            var cartLine = CartLines.FirstOrDefault(x => x.Product.Id == product.Id);
            if (cartLine != null)
            {
                cartLine.Quantity += quantity;
            }
            else
            {
                cartLine = new CartItem
                {
                    Product = product,
                    Quantity = quantity,
                };
                CartLines.Add(cartLine);
            }
        }

        public void Remove(Cart cart)
        {
            _context.Carts.Remove(cart);
        }

        public void RemoveProduct(Product product)
        {
            CartLines.RemoveAll(x => x.Product.Id == product.Id);
        }

        public decimal TotalPrice()
        {
            return CartLines.Sum(x => x.Product.Price * x.Quantity);
        }

        public void UpdateProduct(Product product, int quantity)
        {
            var cartLine = CartLines.FirstOrDefault(x => x.Product.Id == product.Id);
            if (cartLine != null)
            {
                cartLine.Quantity = quantity;
            }
            else
            {
                cartLine = new CartItem
                {
                    Product = product,
                    Quantity = quantity,
                };
                CartLines.Add(cartLine);
            }
        }
        public async Task<Cart> GetCartById(string customerId)
        {
            var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .Where(c => c.CustomerId == customerId)
            .FirstOrDefaultAsync();

            return cart;
        }
    }
}
