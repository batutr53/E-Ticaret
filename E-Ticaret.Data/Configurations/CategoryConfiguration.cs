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
            builder.HasData(new Category
            {
                Id = 1,
                ClassImage = "fa fa-home",
                Name = "Kategoriler",
                Description = "Kategoriler",
                IsActive = true,
                IsTopMenu = true,
                Image = "",
                ParentId = 0,
                OrderNo = 1,
                CreatedDate = new DateTime(2024, 04, 01, 10, 0, 0, DateTimeKind.Utc)
            }, new Category
            {
                Id = 2,
                ClassImage = "fa fa-gift",
                Name = "Hediyelik",
                Description = "Özel günler ve kutlamalar için hediyelik ürünler",
                IsActive = true,
                IsTopMenu = true,
                Image = "",
                ParentId = 1,
                OrderNo = 2,
                CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
            }, new Category
            {
                Id = 3,
                ClassImage = "fa-solid fa-seedling",
                Name = "Canlı Çiçekler",
                Description = "Canlı Çiçekler",
                IsActive = true,
                IsTopMenu = true,
                Image = "",
                ParentId = 1,
                OrderNo = 1,
                CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
            }, new Category
            {
                Id = 4,
                ClassImage = "fa-solid fa-leaf",
                Name = "Yapay Çiçekler",
                Description = "Yapay Çiçekler",
                IsActive = true,
                IsTopMenu = true,
                Image = "",
                ParentId = 1,
                OrderNo = 0,
                CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
            }
            , new Category
              {
                  Id = 5,
                  ClassImage = "fa-solid fa-gift",
                  Name = "KADINLAR GÜNÜ ÇİÇEKLERİ",
                  Description = "KADINLAR GÜNÜ ÇİÇEKLERİ",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              } , new Category
              {
                  Id = 6,
                  ClassImage = "fa-solid fa-leaf",
                  Name = "ARANJMANLAR",
                  Description = "ARANJMANLAR",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 7,
                  ClassImage = "fa-solid fa-leaf",
                  Name = "ORKİDELER",
                  Description = "ORKİDELER",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 8,
                  ClassImage = "fa-solid fa-fan",
                  Name = "ÇİÇEK BUKETLERİ",
                  Description = "ÇİÇEK BUKETLERİ",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 9,
                  ClassImage = "fa-solid fa-seedling",
                  Name = "Canlı Aranjmanlar",
                  Description = "Canlı Aranjmanlar",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 10,
                  ClassImage = "fa-solid fa-heart",
                  Name = "Gül Aranjmanları",
                  Description = "Gül Aranjmanları",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 11,
                  ClassImage = "fa-solid fa-leaf",
                  Name = "Yapay Aranjmanlar",
                  Description = "Yapay Aranjmanlar",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              }, new Category
              {
                  Id = 12,
                  ClassImage = "fa-solid fa-leaf",
                  Name = "Yapay İç Dekorasyon",
                  Description = "Yapay İç Dekorasyon",
                  IsActive = true,
                  IsTopMenu = true,
                  Image = "",
                  ParentId = 1,
                  OrderNo = 0,
                  CreatedDate = new DateTime(2024, 04, 01, 10, 5, 0, DateTimeKind.Utc)
              });
        }
    }
}
