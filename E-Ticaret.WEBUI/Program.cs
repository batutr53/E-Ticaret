using E_Ticaret.Core.Email;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using E_Ticaret.Service.Helpers;
using E_Ticaret.WEBUI.Controllers;
using E_Ticaret.WEBUI.Extensions;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.Text;
using System.Security.Claims;
using System.Text;
using E_Ticaret.WEBUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Image processing service
builder.Services.AddScoped<IImageService, ImageService>();

var cultureInfo = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "E-Ticaret.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
});
builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped(typeof(IService<>),typeof(Service<>));
builder.Services.AddScoped(typeof(IOrderService), typeof(OrderService));
builder.Services.AddScoped(typeof(ICartService), typeof(CartService));
builder.Services.AddScoped(typeof(IProductService), typeof(ProductService));
builder.Services.AddScoped(typeof(IFooterService), typeof(FooterService));
builder.Services.AddScoped(typeof(IFooterContactService), typeof(FooterContactService));
builder.Services.AddScoped(typeof(IFooterMobileMenuService), typeof(FooterMobileMenuService));

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
builder.Services.AddSingleton<TelegramHelper>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Auth/Login";
    options.AccessDeniedPath = "/Admin/Auth/Login";
    options.Events.OnRedirectToLogin = context =>
    {
        // API/AJAX istekleri i�in y�nlendirme yapma
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddMemoryCache();

// Response Caching servisini ekle
builder.Services.AddResponseCaching();

builder.Services.AddAuthorization();

builder.Services.AddScoped<JwtHelper>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role,"Admin"));
    options.AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "Admin","User","Customer"));
}); 

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate(); // otomatik update-database yapar
}

// URL yönlendirme middleware'ini ekle
app.UseMiddleware<UrlRedirectMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


app.UseRouting();
app.UseStaticFiles();
app.UseSession();
app.UseResponseCaching();

app.UseMiddleware<AdminRedirectMiddleware>();
app.UseMiddleware<JwtFromCookieMiddleware>();
app.UseMiddleware<AdminExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}");

// Product Route - /product/detail?productCode=178
app.MapGet("/product/detail", async (HttpContext context, [FromServices] IProductService productService, ILogger<ProductController> logger, [FromQuery] string productCode) =>
{
    if (string.IsNullOrEmpty(productCode))
    {
        logger.LogWarning("Product code is missing");
        return Results.BadRequest("Product code is required");
    }

    var product = await productService.GetProductByCodeAsync(productCode);
    if (product == null)
    {
        logger.LogWarning("Product not found with code: {ProductCode}", productCode);
        return Results.NotFound();
    }

    // SEO URL'ye yönlendir
    var seoUrl = product.Name.ToUrlFriendly();
    return Results.Redirect($"/urun/{product.Id}/{seoUrl}");
});

// SEO-friendly Category Route - /kategori/30/category-name
// Bu route CategoryController'daki [Route("kategori")] attribute'ü ile çakışıyor
// Bu yüzden bu route'u kaldırıyoruz, çünkü controller'da zaten tanımlı

// Eski URL'leri yeni URL'lere yönlendir
app.MapGet("/category", async (HttpContext context, int? id) =>
{
    if (id.HasValue)
    {
        // Eğer id parametresi varsa, yeni URL'ye yönlendir
        return Results.Redirect($"/kategori/{id}");
    }
    // Eğer id yoksa, ana kategori sayfasına yönlendir
    return Results.Redirect("/");
});

// Robots.txt endpoint
app.MapGet("/robots.txt", (HttpContext context) => 
{
    var sb = new StringBuilder();
    sb.AppendLine("User-agent: *");
    sb.AppendLine("Allow: /");
    sb.AppendLine("Disallow: /admin/");
    sb.AppendLine("Disallow: /account/");
    sb.AppendLine("Disallow: /cart/");
    sb.AppendLine("Disallow: /checkout/");
    sb.AppendLine("Disallow: /search/");
    sb.AppendLine("Disallow: /*?*view=");
    sb.AppendLine("Disallow: /*?*sort=");
    sb.AppendLine("Disallow: /*?*page=\n");
    sb.AppendLine($"Sitemap: {context.Request.Scheme}://{context.Request.Host}/sitemap.xml");
    
    return Results.Text(sb.ToString(), "text/plain", Encoding.UTF8);
});

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
