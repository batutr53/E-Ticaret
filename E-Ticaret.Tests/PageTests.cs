using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Ticaret.Tests;

public class PageTests
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
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, AdminAuthentication>("Test", _ => { });
            });
        }
    }

    private sealed class AdminAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test admin"), new Claim(ClaimTypes.Role, "Admin") }, "Test");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }

    [Fact]
    public async Task CheckoutAndNewAdminPagesRenderWithRealRazorViews()
    {
        var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync(fee: 175.50m);
        db.DeliveryTimeRanges.Add(new DeliveryTimeRange { RangeText = "09:00 - 12:00", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), IsActive = true });
        await db.SaveChangesAsync();
        await using var factory = new Factory(db);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", "customerId=test-customer");
        foreach (var path in new[] { "/Order", "/Admin/DeliveryRegions", "/Admin/Notifications", "/Admin/DeliveryRegions/Edit" })
        {
            var response = await client.GetAsync(path);
            var html = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{path}: {response.StatusCode} {html[..Math.Min(html.Length, 2000)]}");
            if (path == "/Order")
            {
                Assert.Contains("name=\"CardMessage\"", html);
                Assert.Contains("name=\"Description\"", html);
                Assert.Contains("name=\"ProvinceId\"", html);
                Assert.Contains("name=\"DistrictId\"", html);
                Assert.Contains("name=\"NeighborhoodId\"", html);
                Assert.DoesNotContain("name=\"DeliveryRegionId\"", html);
                var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(html);
                Assert.Equal("34", document.QuerySelector("#deliveryProvinceId option[value='34']")!.GetAttribute("value"));
                Assert.Equal(2001m, decimal.Parse(document.QuerySelector("[data-subtotal]")!.GetAttribute("data-subtotal")!, System.Globalization.CultureInfo.InvariantCulture));
                Assert.Contains("__RequestVerificationToken", html);
            }
        }
        var regionsHtml = await client.GetStringAsync("/Admin/DeliveryRegions/Edit");
        var regionsDocument = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(regionsHtml);
        var token = regionsDocument.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;
        var saved = await client.PostAsync("/Admin/DeliveryRegions/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token, ["name"] = "Uzak Bölge", ["fee"] = "250,75", ["isActive"] = "false", ["NeighborhoodIds"] = "98689", ["id"] = db.DeliveryRegions.Single().Id.ToString()
        }));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        db.ChangeTracker.Clear();
        Assert.Equal(250.75m, db.DeliveryRegions.Single(x => x.Name == "Uzak Bölge").Fee);
    }
}
