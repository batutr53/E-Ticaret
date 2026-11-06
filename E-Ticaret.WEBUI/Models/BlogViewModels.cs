using System.ComponentModel.DataAnnotations;
using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Models;

public class BlogIndexViewModel
{
    public BlogSetting Settings { get; set; } = new();
    public List<BlogPost> Posts { get; set; } = new();
    public List<BlogCategory> Categories { get; set; } = new();
    public string? CategorySlug { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; } = 12;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class BlogDetailViewModel
{
    public BlogSetting Settings { get; set; } = new();
    public BlogPost Post { get; set; } = new();
    public List<BlogPost> RelatedPosts { get; set; } = new();
    public bool IsPreview { get; set; }
}

public class BlogEditViewModel
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Yazı başlığını girin."), StringLength(200)] public string Title { get; set; } = "";
    [StringLength(220)] public string? Slug { get; set; }
    [StringLength(400)] public string? Excerpt { get; set; }
    [StringLength(100000)] public string? BodyHtml { get; set; }
    public IFormFile? Cover { get; set; }
    public string? ExistingCover { get; set; }
    [StringLength(200)] public string? CoverAlt { get; set; }
    [StringLength(100)] public string? Author { get; set; } = "Detay Çiçek Ekibi";
    [StringLength(70)] public string? SeoTitle { get; set; }
    [StringLength(170)] public string? SeoDescription { get; set; }
    public int? BlogCategoryId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishAtLocal { get; set; }
    public bool SlugLocked { get; set; }
    public long Version { get; set; }
    public List<BlogCategory> Categories { get; set; } = new();
}
