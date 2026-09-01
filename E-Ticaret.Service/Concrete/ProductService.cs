using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete
{
    public class ProductService : IProductService
    {
        private readonly DatabaseContext _context;

        public ProductService(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<List<Product>> GetFilteredProductsAsync(string searchQuery, string sort = "default")
        {
            IQueryable<Product> queryable = _context.Products
                .Where(x => x.IsActive &&
                            (EF.Functions.ILike(x.Name, $"%{searchQuery}%") ||
                             EF.Functions.ILike(x.ProductCode.ToString(), $"%{searchQuery}%")))
                .Include(x => x.Brand)
                .Include(p => p.ProductImages)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category);

            queryable = sort switch
            {
                "price-asc" => queryable.OrderBy(x => x.Price).ThenByDescending(x => x.OrderNo),
                "price-desc" => queryable.OrderByDescending(x => x.Price).ThenByDescending(x => x.OrderNo),
                _ => queryable.OrderByDescending(x => x.OrderNo)
            };

            return await queryable.ToListAsync();
        }
        public async Task<Product> GetProductDetailAsync(int productCode)
        {
            return await _context.Products
                .Include(x => x.Brand)
                .Include(p => p.ProductImages)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive);
        }

        public async Task<List<Product>> GetRelatedProductsAsync(Product product)
        {
            var relatedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();

            return await _context.Products
                .Where(x => x.IsActive && x.Id != product.Id &&
                            x.ProductCategories.Any(pc => relatedCategoryIds.Contains(pc.CategoryId)))
                .Include(x => x.Brand)
                .Include(x => x.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .Include(x=>x.ProductImages)
                .ToListAsync();
        }

    }
}
