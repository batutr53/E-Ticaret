using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150).HasColumnType("character varying(150)");
            builder.Property(x => x.Description).IsRequired().HasMaxLength(500).HasColumnType("character varying(500)");
            builder.Property(x => x.ProductCode).IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            builder.Property(x => x.Price).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.Stock).IsRequired();
            builder.Property(x => x.Image).IsRequired().HasMaxLength(250).HasColumnType("character varying(250)");

        }
    }
}
