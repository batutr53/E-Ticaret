using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AngleSharp.Html.Parser;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Ticaret.Tests;

public class BlogTests
{
    private sealed class Factory(TestDatabase db) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var worker = services.SingleOrDefault(x => x.ImplementationType == typeof(TelegramNotificationWorker));
                if (worker != null) services.Remove(worker);
                services.AddSingleton<DatabaseContext>(db);
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "BlogTest";
                    options.DefaultChallengeScheme = "BlogTest";
                    options.DefaultForbidScheme = "BlogTest";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthentication>("BlogTest", _ => { });
            });
        }
    }
    private sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers["X-Test-Role"].ToString();
            if (string.IsNullOrEmpty(role)) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Blog test"), new Claim(ClaimTypes.Role, role) }, "BlogTest");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "BlogTest")));
        }
    }
    private static HttpClient Client(Factory factory, string? role = null)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        if (role != null) client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }
    private static async Task<string> Token(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        return new HtmlParser().ParseDocument(html).QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;
    }
    private static async Task Seed(TestDatabase db)
    {
        var active = new BlogCategory { Name = "Çiçek Bakımı", Slug = "cicek-bakimi" };
        var closed = new BlogCategory { Name = "Kapalı Kategori", Slug = "kapali", IsActive = false };
        db.BlogCategories.AddRange(active, closed);
        for (var i = 0; i < 14; i++)
        {
            db.BlogPosts.Add(new BlogPost { Title = $"İlham yazısı {i}", Slug = $"ilham-{i}", BodyHtml = "<h2>Çiçeklere küçük dokunuşlar</h2><p>Çiçeklerinizin hikâyesini küçük ayrıntılarla paylaşın.</p>",
                Excerpt = "Çiçeklerden ilham alan küçük öneriler ve özel anlar için fikirler.", IsPublished = true, PublishedAt = DateTime.UtcNow.AddDays(-i - 1), BlogCategory = active, CoverImage = "placeholder.svg" });
        }
        db.BlogPosts.AddRange(
            new BlogPost { Title = "Gizli taslak", Slug = "gizli-taslak", BodyHtml = "<p>Taslak içerik</p>" },
            new BlogPost { Title = "Gelecek yazı", Slug = "gelecek", IsPublished = true, PublishedAt = DateTime.UtcNow.AddDays(7) },
            new BlogPost { Title = "Kapalı yazı", Slug = "kapali-yazi", IsPublished = true, PublishedAt = DateTime.UtcNow.AddDays(-1), BlogCategory = closed });
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
    }

    [Fact]
    public async Task PublicPagesPaginationSearchSeoAndPreviewVisibilityWork()
    {
        await using var db = await TestDatabase.CreateAsync();
        await Seed(db);
        await using var factory = new Factory(db);
        using var client = Client(factory);
        var response = await client.GetAsync("/blog");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlParser().ParseDocument(html);
        Assert.Equal(12, document.QuerySelectorAll(".journal-card").Length);
        Assert.DoesNotContain("Gizli taslak", html);
        Assert.DoesNotContain("Gelecek yazı", html);
        Assert.DoesNotContain("Kapalı yazı", html);
        Assert.Equal("https://www.detaycicek.com/blog", document.QuerySelector("link[rel='canonical']")!.GetAttribute("href"));
        var second = new HtmlParser().ParseDocument(await client.GetStringAsync("/blog?page=2"));
        Assert.Equal(2, second.QuerySelectorAll(".journal-card").Length);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/blog?page=3")).StatusCode);
        foreach (var path in new[] { "/blog/gizli-taslak", "/blog/gelecek", "/blog/kapali-yazi", "/blog?category=kapali", "/blog?category=missing", "/blog?page=-1" })
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
        var search = new HtmlParser().ParseDocument(await client.GetStringAsync("/blog?q=İlham"));
        Assert.Equal("noindex,follow", search.QuerySelector("meta[name='robots']")!.GetAttribute("content"));
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Admin/Blog/Preview/1")).StatusCode);
        var detail = new HtmlParser().ParseDocument(await client.GetStringAsync("/blog/ilham-0"));
        Assert.Single(detail.QuerySelectorAll("h1"));
        Assert.Single(detail.QuerySelectorAll("meta[name='description']"));
        Assert.Contains("BlogPosting", detail.QuerySelector("script[type='application/ld+json']")!.TextContent);
        Assert.Equal(3, detail.QuerySelectorAll(".journal-related .journal-card").Length);
        var sitemap = await client.GetStringAsync("/blog/sitemap.xml");
        Assert.Contains("ilham-0", sitemap); Assert.DoesNotContain("gizli-taslak", sitemap); Assert.DoesNotContain("gelecek", sitemap); Assert.DoesNotContain("kapali-yazi", sitemap);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Customer")]
    public async Task EveryAdminActionRequiresAdmin(string? role)
    {
        await using var db = await TestDatabase.CreateAsync();
        await using var factory = new Factory(db);
        using var client = Client(factory, role);
        foreach (var path in new[] { "/Admin/Blog", "/Admin/Blog/Edit", "/Admin/Blog/Categories", "/Admin/Blog/Settings", "/Admin/Blog/Preview/1" })
        {
            var response = await client.GetAsync(path);
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.Redirect, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized });
        }
        foreach (var path in new[] { "Save", "Unpublish", "UploadImage", "SaveCategory", "Settings" })
        {
            var response = await client.PostAsync("/Admin/Blog/" + path, new FormUrlEncodedContent(new Dictionary<string, string>()));
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }
        Assert.Empty(await db.BlogPosts.ToListAsync());
    }

    [Fact]
    public async Task EditorSavesSanitizedDraftPublishesSchedulesAndPreservesCoverAndSlug()
    {
        await using var db = await TestDatabase.CreateAsync();
        await using var factory = new Factory(db);
        using var client = Client(factory, "Admin");
        var token = await Token(client, "/Admin/Blog/Edit");
        var fields = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["Title"] = "Çiçeklerin Güzel Dünyası", ["BodyHtml"] = "<h2>Çiçekler</h2><p>İçerik en az yirmi karakterdir.</p><script>alert('x')</script>", ["intent"] = "draft" };
        var noToken = new Dictionary<string, string>(fields); noToken.Remove("__RequestVerificationToken");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(noToken))).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear();
        var post = await db.BlogPosts.AsNoTracking().SingleAsync();
        Assert.Equal("ciceklerin-guzel-dunyasi", post.Slug); Assert.False(post.IsPublished); Assert.DoesNotContain("<script", post.BodyHtml);
        using var publicClient = Client(factory);
        Assert.Equal(HttpStatusCode.NotFound, (await publicClient.GetAsync("/blog/" + post.Slug)).StatusCode);
        var preview = await client.GetAsync("/Admin/Blog/Preview/" + post.Id);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.Contains("no-store", preview.Headers.CacheControl!.ToString());
        Assert.Equal("noindex, nofollow", preview.Headers.GetValues("X-Robots-Tag").Single());
        fields["Id"] = post.Id.ToString(); fields["Version"] = post.UpdatedAt.Ticks.ToString(); fields["Slug"] = post.Slug; fields["intent"] = "publish";
        var missingCover = await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, missingCover.StatusCode);
        db.ChangeTracker.Clear(); Assert.False((await db.BlogPosts.AsNoTracking().SingleAsync()).IsPublished);
        // Seed a stored cover as if a validated upload had already completed.
        await db.BlogPosts.ExecuteUpdateAsync(s => s.SetProperty(x => x.CoverImage, "stored-cover.jpg"));
        fields["PublishAtLocal"] = BlogContent.LocalDate(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-ddTHH:mm");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear(); post = await db.BlogPosts.AsNoTracking().SingleAsync();
        Assert.True(post.IsPublished); Assert.Equal("stored-cover.jpg", post.CoverImage);
        Assert.True(post.PublishedAt > DateTime.UtcNow.AddDays(1));
        Assert.Equal(HttpStatusCode.NotFound, (await publicClient.GetAsync("/blog/" + post.Slug)).StatusCode);
        fields["Version"] = post.UpdatedAt.Ticks.ToString(); fields.Remove("PublishAtLocal");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear(); post = await db.BlogPosts.AsNoTracking().SingleAsync();
        Assert.Equal(HttpStatusCode.OK, (await publicClient.GetAsync("/blog/" + post.Slug)).StatusCode);
        fields["Version"] = post.UpdatedAt.Ticks.ToString(); fields["Slug"] = "changed-url";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear(); Assert.Equal(post.Slug, (await db.BlogPosts.AsNoTracking().SingleAsync()).Slug);
        var unpublishToken = await Token(client, "/Admin/Blog");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/Unpublish", new FormUrlEncodedContent(new Dictionary<string, string> { ["id"] = post.Id.ToString(), ["__RequestVerificationToken"] = unpublishToken }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publicClient.GetAsync("/blog/" + post.Slug)).StatusCode);
    }

    [Fact]
    public async Task DuplicateSlugsStaleEditsAndFakeImageUploadsAreRejected()
    {
        await using var db = await TestDatabase.CreateAsync(); await Seed(db);
        await using var factory = new Factory(db); using var client = Client(factory, "Admin");
        var token = await Token(client, "/Admin/Blog/Edit");
        var fields = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["Title"] = "Başka yazı", ["Slug"] = "ilham-0", ["intent"] = "draft" };
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        Assert.Equal(17, await db.BlogPosts.CountAsync());
        var post = await db.BlogPosts.AsNoTracking().FirstAsync();
        fields["Id"] = post.Id.ToString(); fields["Version"] = "1"; fields["Slug"] = post.Slug;
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Admin/Blog/Save", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear(); Assert.NotEqual("Başka yazı", (await db.BlogPosts.FindAsync(post.Id))!.Title);
        using var upload = new MultipartFormDataContent();
        upload.Add(new StringContent(token), "__RequestVerificationToken");
        upload.Add(new ByteArrayContent("<svg onload='alert(1)'/>"u8.ToArray()), "file", "fake.jpg");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Admin/Blog/UploadImage", upload)).StatusCode);
    }

    [Fact]
    public async Task CategoryAndSettingsChangesAreSavedAndCategoryClosureHidesArticles()
    {
        await using var db = await TestDatabase.CreateAsync(); await Seed(db);
        await using var factory = new Factory(db); using var client = Client(factory, "Admin");
        var token = await Token(client, "/Admin/Blog/Categories");
        var category = await db.BlogCategories.AsNoTracking().SingleAsync(x => x.Slug == "cicek-bakimi");
        var fields = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["id"] = category.Id.ToString(), ["name"] = category.Name, ["sortOrder"] = "3", ["isActive"] = "false" };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/SaveCategory", new FormUrlEncodedContent(fields))).StatusCode);
        db.ChangeTracker.Clear(); Assert.False((await db.BlogCategories.AsNoTracking().SingleAsync(x => x.Id == category.Id)).IsActive);
        using var visitor = Client(factory);
        Assert.Equal(HttpStatusCode.NotFound, (await visitor.GetAsync("/blog/ilham-0")).StatusCode);
        var settingsToken = await Token(client, "/Admin/Blog/Settings");
        var settings = new Dictionary<string, string> { ["__RequestVerificationToken"] = settingsToken, ["Title"] = "Çiçek Günlüğü", ["Description"] = "İlham veren yeni fikirler." };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Admin/Blog/Settings", new FormUrlEncodedContent(settings))).StatusCode);
        db.ChangeTracker.Clear(); Assert.Equal("Çiçek Günlüğü", (await db.BlogSettings.AsNoTracking().SingleAsync()).Title);
        var html = new HtmlParser().ParseDocument(await visitor.GetStringAsync("/blog"));
        Assert.Equal("Çiçek Günlüğü", html.QuerySelector("h1")!.TextContent);
        Assert.Empty(html.QuerySelectorAll(".journal-card"));
    }

    [Fact]
    public async Task AdminPagesRenderAndOptionalLocalPreviewArtifactsUseRealRazor()
    {
        await using var db = await TestDatabase.CreateAsync(); await Seed(db);
        // Only test DB content: no example articles are inserted into the shop database.
        var titles = new[] { "Çiçeklerle yaşam alanlarınıza küçük bir dokunuş", "Sevdikleriniz için anlamlı bir çiçek seçimi", "Vazoda çiçek düzenlemenin incelikleri", "Hediye notlarınıza ilham veren fikirler", "Sofralara renk katan çiçekler", "Özel günler için zarif bir sürpriz" };
        var posts = await db.BlogPosts.OrderBy(x => x.Id).Take(6).ToListAsync();
        for (var i = 0; i < posts.Count; i++) posts[i].Title = titles[i];
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        await using var factory = new Factory(db); using var client = Client(factory, "Admin");
        foreach (var path in new[] { "/blog", "/blog/ilham-0", "/Admin/Blog", "/Admin/Blog/Edit/1", "/Admin/Blog/Edit", "/Admin/Blog/Categories", "/Admin/Blog/Settings" })
        {
            var response = await client.GetAsync(path); var html = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{path}: {response.StatusCode} {html[..Math.Min(800, html.Length)]}");
            var export = Environment.GetEnvironmentVariable("DTYC_BLOG_PREVIEW_DIR");
            if (!string.IsNullOrEmpty(export))
            {
                Directory.CreateDirectory(export);
                var name = path.Trim('/').Replace('/', '-') + ".html";
                await File.WriteAllTextAsync(Path.Combine(export, name), html);
            }
        }
    }
}
