using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace E_Ticaret.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<DeliveryTimeRange> DeliveryTimeRanges { get; set; }
        public DbSet<FooterSection> FooterSections { get; set; }
        public DbSet<FooterLink> FooterLinks { get; set; }
        public DbSet<FooterContact> FooterContacts { get; set; }
        public DbSet<FooterMobileMenu> FooterMobileMenus { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<MobileBanner> MobileBanners { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogSetting> BlogSettings { get; set; }
        public DbSet<DeliveryRegion> DeliveryRegions { get; set; }
        public DbSet<TelegramNotification> TelegramNotifications { get; set; }
        public DbSet<AddressProvince> AddressProvinces { get; set; }
        public DbSet<AddressDistrict> AddressDistricts { get; set; }
        public DbSet<AddressNeighborhood> AddressNeighborhoods { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=82.153.241.175;Port=5432;Database=eticaretdetaycicekdb;Username=postgres;Password=+12131213Qq1");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<DeliveryRegion>().Property(x => x.Fee).HasPrecision(18, 2);
            modelBuilder.Entity<DeliveryRegion>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<TelegramNotification>().HasIndex(x => x.OrderId).IsUnique();
            modelBuilder.Entity<TelegramNotification>().HasIndex(x => new { x.SentAt, x.NextAttemptAt });
            modelBuilder.Entity<TelegramNotification>().HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId);
            modelBuilder.Entity<Order>().Property(x => x.CardMessage).HasMaxLength(500);
            modelBuilder.Entity<Order>().Property(x => x.DeliveryRegionName).HasMaxLength(120);
            modelBuilder.Entity<AddressProvince>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AddressDistrict>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AddressNeighborhood>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AddressDistrict>().HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AddressNeighborhood>().HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AddressNeighborhood>().HasOne(x => x.DeliveryRegion).WithMany(x => x.Neighborhoods).HasForeignKey(x => x.DeliveryRegionId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<AddressDistrict>().HasIndex(x => new { x.ProvinceId, x.Name });
            modelBuilder.Entity<AddressNeighborhood>().HasIndex(x => new { x.DistrictId, x.Name });
            modelBuilder.Entity<Order>().Property(x => x.DeliveryDistrictName).HasMaxLength(120);
            modelBuilder.Entity<Order>().Property(x => x.DeliveryNeighborhoodName).HasMaxLength(300);
            base.OnModelCreating(modelBuilder);
        }
    }
}
