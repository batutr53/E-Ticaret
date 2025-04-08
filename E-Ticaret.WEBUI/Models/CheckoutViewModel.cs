using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class CheckoutViewModel
    {
        public List<CartLine>? CartProducts { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
