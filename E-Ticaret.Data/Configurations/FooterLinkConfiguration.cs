using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Data.Configurations
{

    public class FooterLinkConfiguration : IEntityTypeConfiguration<FooterLink>
    {
        public void Configure(EntityTypeBuilder<FooterLink> builder)
        {
            builder.ToTable("FooterLinks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnType("character varying(250)");

            builder.Property(x => x.Url)
                .HasMaxLength(500);

            builder.Property(x => x.IconClass)
                .HasMaxLength(100);

            builder.Property(x => x.OrderNo)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.HasOne(x => x.Section)
                .WithMany(x => x.Links)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
