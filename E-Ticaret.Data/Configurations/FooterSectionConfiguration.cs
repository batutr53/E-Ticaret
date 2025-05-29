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
    public class FooterSectionConfiguration : IEntityTypeConfiguration<FooterSection>
    {
        public void Configure(EntityTypeBuilder<FooterSection> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnType("character varying(250)");

            builder.Property(x => x.OrderNo)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            // 1 Section -> N Link ilişkisi
            builder.HasMany(x => x.Links)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
