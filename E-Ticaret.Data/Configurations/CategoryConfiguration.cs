using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Image).IsRequired().HasMaxLength(250).HasColumnType("character varying(250)");
            builder.HasData(new Category {
                Id = 1,
                ClassImage = "fa fa-home",
                Name = "Kategoriler", 
                Description = "Kategoriler", 
                IsActive = true, 
                IsTopMenu = true, 
                ParentId = 0,
                OrderNo = 1, 
                CreatedDate = new DateTime(2024, 04, 01, 10, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
