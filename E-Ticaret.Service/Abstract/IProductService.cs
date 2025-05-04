using E_Ticaret.Core.Entities;

namespace E_Ticaret.Service.Abstract
{
    public interface IProductService
    {
        Task<List<Product>> GetFilteredProductsAsync(string searchQuery);
        Task<Product> GetProductDetailAsync(int productCode);
        Task<List<Product>> GetRelatedProductsAsync(Product product);

    }
}
