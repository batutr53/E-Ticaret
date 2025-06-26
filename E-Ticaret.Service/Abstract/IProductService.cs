using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface IProductService
    {
        Task<List<Product>> GetFilteredProductsAsync(string searchQuery);
        Task<Product> GetProductDetailAsync(int productId);
        Task<Product> GetProductByCodeAsync(string productCode);
        Task<List<Product>> GetRelatedProductsAsync(Product product);

    }
}
