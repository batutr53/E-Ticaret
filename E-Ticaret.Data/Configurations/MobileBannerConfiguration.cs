using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class MobileBannerConfiguration : IEntityTypeConfiguration<MobileBanner>
    {
        public void Configure(EntityTypeBuilder<MobileBanner> builder)
        {
            builder.Property(x => x.Image).HasMaxLength(250);
            builder.Property(x => x.Link).HasMaxLength(500);
            builder.HasIndex(x => x.OrderNo);
        }
    }
}
