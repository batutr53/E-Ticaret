using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models
{
    public class ProductDetailViewModel
    {
        public Product? Product { get; set; }
        public List<Product>? RelatedProducts { get; set; }
    }
}
