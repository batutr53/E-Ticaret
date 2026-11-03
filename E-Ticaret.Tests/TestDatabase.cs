using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Tests;

internal sealed class TestDatabase : DatabaseContext
{
    private TestDatabase(DbContextOptions<DatabaseContext> options) : base(options) { }
    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var db = new TestDatabase(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Product>().Property(x => x.ProductCode).HasDefaultValueSql(null);
        builder.Entity<DeliveryTimeRange>().Property(x => x.StartTime).HasConversion(x => x.Ticks, x => new TimeSpan(x));
        builder.Entity<DeliveryTimeRange>().Property(x => x.EndTime).HasConversion(x => x.Ticks, x => new TimeSpan(x));
    }
    public async Task<Product> SeedCartAsync(bool active = true)
    {
        var product = new Product
        {
            Name = "Test Çiçek", Description = "Test açıklaması", Price = 1000.50m, BrandId = 1,
            ProductCode = 100001, Stock = 50, IsActive = active,
            ProductCategories = new List<ProductCategory> { new() { CategoryId = 2 } },
            ProductImages = new List<ProductImage> { new() { ImageUrl = "test.jpg", IsDefault = true } }
        };
        Carts.Add(new Cart { CustomerId = "test-customer", CartItems = new List<CartItem> { new() { Product = product, Quantity = 2 } } });
        await SaveChangesAsync();
        return product;
    }

    public async Task<AddressNeighborhood> SeedAddressAsync(decimal fee = 150.25m, bool active = true, bool mapped = true)
    {
        var province = new AddressProvince { Id = 34, Name = "İstanbul" };
        var district = new AddressDistrict { Id = 1852, Name = "Ümraniye", Province = province };
        var neighborhood = new AddressNeighborhood { Id = 98689, Name = "Çamlık", District = district,
            DeliveryRegion = mapped ? new DeliveryRegion { Name = "Yakın teslimat", Fee = fee, IsActive = active } : null };
        AddressNeighborhoods.Add(neighborhood);
        await SaveChangesAsync();
        return neighborhood;
    }
}
