using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.Entities
{
    public class Cart : IEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = null!;
        public List<CartItem> CartItems { get; set; } = new();

        public void AddItem(Product product, int quantity)
        {
            var cartItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (cartItem == null)
            {
                CartItems.Add(new CartItem { ProductId = product.Id, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

        }

        public void DeleteItem(int productId,int quantity)
        {
            var cartItem = CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem == null) return;

            cartItem.Quantity -= quantity;

            if (cartItem.Quantity <= 0)
            {
                CartItems.Remove(cartItem);
            }
        }
    }

    public class CartItem : IEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int Quantity { get; set; }
    }
}
