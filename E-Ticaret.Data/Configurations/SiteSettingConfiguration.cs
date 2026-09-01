using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
    {
        public void Configure(EntityTypeBuilder<SiteSetting> builder)
        {
            builder.ToTable(t => t.HasCheckConstraint("CK_SiteSettings_Singleton", "\"Id\" = 1"));
            builder.Property(x => x.Logo).HasMaxLength(250);
            builder.Property(x => x.PrimaryColor).HasMaxLength(7).IsRequired();
            builder.Property(x => x.AccentColor).HasMaxLength(7).IsRequired();
            builder.HasData(new SiteSetting
            {
                Id = SiteSetting.SingletonId,
                PrimaryColor = "#888888",
                AccentColor = "#236B43"
            });
        }
    }
}
