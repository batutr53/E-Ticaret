using System.ComponentModel.DataAnnotations;

namespace E_Ticaret.Core.Entities;

public class BlogCategory
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(120)] public string Slug { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BlogPost
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = "";
    [Required, MaxLength(220)] public string Slug { get; set; } = "";
    [MaxLength(400)] public string Excerpt { get; set; } = "";
    public string BodyHtml { get; set; } = "";
    [MaxLength(255)] public string? CoverImage { get; set; }
    [MaxLength(200)] public string CoverAlt { get; set; } = "";
    [MaxLength(100)] public string Author { get; set; } = "Detay Çiçek Ekibi";
    [MaxLength(70)] public string? SeoTitle { get; set; }
    [MaxLength(170)] public string? SeoDescription { get; set; }
    public int? BlogCategoryId { get; set; }
    public BlogCategory? BlogCategory { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class BlogSetting
{
    public int Id { get; set; } = 1;
    [Required, MaxLength(100)] public string Title { get; set; } = "Blog";
    [MaxLength(250)] public string Description { get; set; } = "Çiçeklerin dünyasından ilham, bakım önerileri ve özel günler için fikirler.";
    [MaxLength(255)] public string? BannerImage { get; set; }
}
