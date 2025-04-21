using E_Ticaret.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.DTO
{
    public class CartDto
    {
        public List<Cart>? Carts { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
