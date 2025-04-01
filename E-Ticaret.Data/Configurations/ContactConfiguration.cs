using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Surname).IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Email).HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Phone).HasColumnType("character varying(20)").HasMaxLength(20);
            builder.Property(x => x.Subject).HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Message).HasMaxLength(500).HasColumnType("character varying(500)").IsRequired();
        }
    }
}
