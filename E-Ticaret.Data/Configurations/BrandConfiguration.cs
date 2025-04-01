using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Data.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.Property(x=>x.Name).IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            builder.Property(x => x.Description).HasMaxLength(200).HasColumnType("character varying(200)");
            builder.Property(x => x.Logo).HasMaxLength(250).HasColumnType("character varying(250)");

        }
    }
}
