using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Description).HasMaxLength(200).HasColumnType("character varying(200)");
            builder.Property(x => x.Logo).HasMaxLength(250).HasColumnType("character varying(250)");
            builder.HasData(new Brand
            {
                Id = 1,
                Name = "Detay Çiçek",
                Description = "Detay Çiçek",
                Logo = "",
                IsActive = true,
                CreatedDate = new DateTime(2024, 04, 01, 10, 0, 0, DateTimeKind.Utc)
            });

        }
    }
}
