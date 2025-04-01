using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(100).HasColumnType("character varying(100)").IsRequired();
            builder.Property(x => x.Surname).HasMaxLength(100).HasColumnType("character varying(100)").IsRequired();
            builder.Property(x => x.Email).HasMaxLength(100).HasColumnType("character varying(100)").IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(15).HasColumnType("character varying(15)");
            builder.Property(x => x.Password).HasMaxLength(100).HasColumnType("character varying(100)").IsRequired();
            builder.Property(x => x.UserName).HasMaxLength(100).HasColumnType("character varying(100)");
            builder.HasData(new AppUser
            {
                Id = 1,
                Name = "Admin",
                Surname = "Admin",
                Email = "info@detaycicek.com",
                IsActive = true,
                IsAdmin = true,
                Password = "314120",
                UserName = "admin",
                 UserGuid = new Guid("11111111-1111-1111-1111-111111111111"),
                CreatedDate = new DateTime(2024, 04, 01, 10, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
