using System.Net;
using System.Text.RegularExpressions;
using E_Ticaret.Core.Entities;

namespace E_Ticaret.WEBUI.Helpers;

public static class BlogContent
{
    public static IQueryable<BlogPost> Published(this IQueryable<BlogPost> query, DateTime now) => query.Where(x =>
        x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now && (x.BlogCategoryId == null || x.BlogCategory!.IsActive));

    public static string PlainText(string? html) => Regex.Replace(WebUtility.HtmlDecode(
        Regex.Replace(html ?? "", "<[^>]+>", " ")), @"\s+", " ").Trim();
    public static int ReadingMinutes(string? html) => Math.Max(1, (int)Math.Ceiling(PlainText(html).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 200d));
    public static string Excerpt(BlogPost post) => string.IsNullOrWhiteSpace(post.Excerpt)
        ? Shorten(PlainText(post.BodyHtml), 160) : post.Excerpt;
    public static string Shorten(string text, int max) => text.Length <= max ? text : text[..max].TrimEnd() + "…";
    public static TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    public static DateTime LocalDate(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone);
    public static DateTime UtcDate(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZone);
    public static string CoverUrl(string? filename) => string.IsNullOrWhiteSpace(filename) ? "/img/blog/placeholder.svg" : "/img/blog/" + Uri.EscapeDataString(filename);
}
