using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class CartViewModel
    {
        public List<Cart>? Carts{ get; set; }
        public decimal TotalPrice { get; set; }

    }
}
