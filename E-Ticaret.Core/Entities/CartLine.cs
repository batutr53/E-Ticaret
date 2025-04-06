using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.Entities
{
    public class CartLine
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public Product Product { get; set; }
        public CartLine()
        {
            Quantity = 1;
        }
        public decimal TotalPrice()
        {
            return Price * Quantity;
        }
    }
}
