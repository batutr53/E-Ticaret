using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface IProductService
    {
        Task<List<Product>> GetFilteredProductsAsync(string searchQuery, string sort = "default");
        Task<Product> GetProductDetailAsync(int productCode);
        Task<List<Product>> GetRelatedProductsAsync(Product product);

    }
}
