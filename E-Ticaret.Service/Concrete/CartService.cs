using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;

namespace E_Ticaret.Service.Concrete
{
    public class CartService : ICartService
    {
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

        public void ClearAll()
        {
            CartLines.Clear();
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
    }
}
