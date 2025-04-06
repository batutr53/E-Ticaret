using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Service.Concrete
{
    public class CartService : ICartService
    {
        public List<CartLine> CartLines = new();
        public void AddToProduct(Product product, int quantity)
        {
           var cartLine = CartLines.FirstOrDefault(x => x.Product.Id == product.Id);
            if (cartLine != null)
            {
                cartLine.Quantity += quantity;
            }
            else
            {
                cartLine = new CartLine
                {
                    Product = product,
                    Quantity = quantity,
                    Price = product.Price
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
            return CartLines.Sum(x => x.TotalPrice());
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
                cartLine = new CartLine
                {
                    Product = product,
                    Quantity = quantity,
                    Price = product.Price
                };
                CartLines.Add(cartLine);
            }
        }
    }
}
