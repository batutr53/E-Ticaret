using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations;

public class BlogConfiguration : IEntityTypeConfiguration<BlogPost>, IEntityTypeConfiguration<BlogCategory>, IEntityTypeConfiguration<BlogSetting>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.IsPublished, x.PublishedAt });
        builder.Property(x => x.UpdatedAt).IsConcurrencyToken();
        builder.HasOne(x => x.BlogCategory).WithMany().HasForeignKey(x => x.BlogCategoryId).OnDelete(DeleteBehavior.Restrict);
    }
    public void Configure(EntityTypeBuilder<BlogCategory> builder) => builder.HasIndex(x => x.Slug).IsUnique();
    public void Configure(EntityTypeBuilder<BlogSetting> builder) => builder.HasData(new BlogSetting { Id = 1 });
}
