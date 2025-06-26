using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Extensions;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace E_Ticaret.WEBUI.Controllers
{
    public class ProductController : Controller
    {
        private string GenerateSimpleProductSchema(Product product)
        {
            try
            {
                // Base URL'i al
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request != null 
                    ? $"{request.Scheme}://{request.Host}{request.PathBase}" 
                    : string.Empty;
                
                // Ürünün ilk resmini al
                var mainImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl;
                var imageUrl = !string.IsNullOrEmpty(mainImageUrl)
                    ? new Uri(new Uri(baseUrl), mainImageUrl.TrimStart('/')).ToString()
                    : new Uri(new Uri(baseUrl), "/img/no-image.png").ToString();

                // SEO dostu URL oluştur
                var seoUrl = product.Name?.ToUrlFriendly() ?? string.Empty;
                var productUrl = !string.IsNullOrEmpty(baseUrl)
                    ? new Uri(new Uri(baseUrl), $"urun/{product.Id}/{seoUrl}").ToString()
                    : string.Empty;

                // Schema.org uyumlu JSON oluştur
                var schema = new Dictionary<string, object>
                {
                    ["@context"] = "https://schema.org",
                    ["@type"] = "Product",
                    ["name"] = product.Name ?? string.Empty,
                    ["description"] = !string.IsNullOrEmpty(product.Description) && product.Description.Length > 160 
                        ? product.Description.Substring(0, 157) + "..." 
                        : product.Description ?? string.Empty,
                    ["image"] = imageUrl,
                    ["offers"] = new Dictionary<string, object>
                    {
                        ["@type"] = "Offer",
                        ["url"] = productUrl,
                        ["priceCurrency"] = "TRY",
                        ["price"] = product.Price,
                        ["availability"] = product.Stock > 0 
                            ? "https://schema.org/InStock" 
                            : "https://schema.org/OutOfStock"
                    }
                };

                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Serialize(schema, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schema oluşturulurken bir hata oluştu. ProductId: {ProductId}", product.Id);
                return string.Empty;
            }
        }

        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(
            IProductService productService, 
            ILogger<ProductController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<IActionResult> Index(string q = "")
        {
            try
            {
                var result = await _productService.GetFilteredProductsAsync(q);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesi yüklenirken bir hata oluştu");
                return StatusCode(500, "Ürün listesi yüklenirken bir hata oluştu.");
            }
        }

        [Route("urun/{id:int}/{seoUrl?}", Name = "ProductDetail")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        public async Task<IActionResult> Detail(int id, string seoUrl = null)
        {
            try
            {
                var product = await _productService.GetProductDetailAsync(id);
                if (product == null) return NotFound();

                // SEO URL doğrulama ve yönlendirme
                var expectedSeoUrl = product.Name.ToUrlFriendly();
                
                if (string.IsNullOrEmpty(seoUrl) || !string.Equals(seoUrl, expectedSeoUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToRoute("ProductDetail", new { id, seoUrl = expectedSeoUrl });
                }

                // ViewBag ile SEO bilgilerini gönder
                ViewBag.PageTitle = product.Name;
                
                // SEO için ViewData ayarları
                ViewData["Title"] = product.Name;
                ViewData["MetaDescription"] = product.Description?.Length > 160 
                    ? product.Description.Substring(0, 157) + "..." 
                    : product.Description;
                ViewData["Keywords"] = string.Join(", ", product.Name, "çiçek", "çiçekçi", "online çiçek");
                ViewData["ImageUrl"] = product.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/img/no-image.png";
                ViewData["PageType"] = "product";
                
                // Schema.org JSON-LD için veri hazırla
                ViewBag.SchemaMarkup = GenerateSimpleProductSchema(product);
                ViewBag.CanonicalUrl = Url.RouteUrl("ProductDetail", new { id, seoUrl = expectedSeoUrl }, Request.Scheme);
                
                // İlgili ürünleri getir
                var relatedProducts = await _productService.GetRelatedProductsAsync(product);
                
                var viewModel = new ProductDetailViewModel 
                { 
                    Product = product, 
                    RelatedProducts = relatedProducts ?? new List<Product>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün detay sayfası yüklenirken bir hata oluştu. ProductId: {ProductId}", id);
                return StatusCode(500, "Ürün detay sayfası yüklenirken bir hata oluştu.");
            }
        }
    }
}
