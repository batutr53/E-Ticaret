namespace E_Ticaret.Core.Entities
{
     public class Product : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public long ProductCode { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public bool IsHome { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public IList<ProductCategory>? ProductCategories { get; set; }
        public IList<ProductImage>? ProductImages { get; set; }
    }
}
