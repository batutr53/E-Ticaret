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
    public class SliderConfiguration : IEntityTypeConfiguration<Slider>
    {
        public void Configure(EntityTypeBuilder<Slider> builder)
        {
            builder.Property(x => x.Title).HasMaxLength(250).HasColumnType("character varying(250)");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.Image).HasMaxLength(250).HasColumnType("character varying(250)");
            builder.Property(x => x.Link).HasMaxLength(200).HasColumnType("character varying(200)");
            builder.Property(x => x.DisplayType).HasConversion<string>().HasMaxLength(20).HasDefaultValue(SliderDisplayType.Desktop);
            builder.Property(x => x.OrderNo).HasDefaultValue(0);
            builder.HasIndex(x => new { x.DisplayType, x.OrderNo });
        }
    }
}
