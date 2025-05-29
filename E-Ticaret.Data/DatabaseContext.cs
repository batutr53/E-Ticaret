using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace E_Ticaret.Data
{
    public class DatabaseContext : DbContext
    {

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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=31.57.33.111;Port=5432;Database=eticaretdetaycicekdb;Username=postgres;Password=123456");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
