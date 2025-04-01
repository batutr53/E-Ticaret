using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class NewsConfiguration : IEntityTypeConfiguration<News>
    {
        public void Configure(EntityTypeBuilder<News> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(250).HasColumnType("character varying(250)");
            builder.Property(x => x.Description).IsRequired().HasMaxLength(750);
            builder.Property(x => x.Image).IsRequired().HasMaxLength(250).HasColumnType("character varying(250)");
        }
    }
}
