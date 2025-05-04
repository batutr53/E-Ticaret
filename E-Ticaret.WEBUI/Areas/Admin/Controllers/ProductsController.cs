using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;
using E_Ticaret.WEBUI.Areas.Admin.Models;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DatabaseContext _context;

        public ProductsController(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductImages) 
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }



        // GET: Admin/Products/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["Categories"] = new MultiSelectList(_context.Categories.Where(x => x.IsActive), "Id", "Name");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> Images, List<int> CategoryIds, string ImageOrder, int DefaultImageIndex)
        {
            if (ModelState.IsValid)
            {
                // 1. Ürün kategorilerini ata
                product.ProductCategories = CategoryIds.Select(id => new ProductCategory
                {
                    CategoryId = id
                }).ToList();

                // 2. Görselleri yükle
                if (Images != null && Images.Any())
                {
                    product.ProductImages = new List<ProductImage>();

                    // Sıralamayı dizine çevir
                    var imageOrderList = !string.IsNullOrEmpty(ImageOrder)
                        ? ImageOrder.Split(',').Select(int.Parse).ToList()
                        : Enumerable.Range(0, Images.Count).ToList();

                    for (int i = 0; i < imageOrderList.Count; i++)
                    {
                        int imageIndex = imageOrderList[i];
                        if (imageIndex >= 0 && imageIndex < Images.Count)
                        {
                            var image = Images[imageIndex];
                            var imagePath = await FileHelper.FileLoaderAsync(image);
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                product.ProductImages.Add(new ProductImage
                                {
                                    ImageUrl = imagePath,
                                    Order = i,
                                    IsDefault = (imageIndex == DefaultImageIndex)
                                });
                            }
                        }
                    }
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["Categories"] = new MultiSelectList(_context.Categories.Where(x => x.IsActive), "Id", "Name");
            return View(product);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductImages) 
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            var selectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();
            ViewData["Categories"] = new MultiSelectList(_context.Categories, "Id", "Name", selectedCategoryIds);

            return View(product);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile>? Images, List<int> CategoryIds, bool cbRemoveImage = false)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .Include(p => p.ProductCategories)
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProduct == null)
                        return NotFound();

                    // Temel alanları güncelle
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;
                    existingProduct.IsHome = product.IsHome;
                    existingProduct.IsActive = product.IsActive;
                    existingProduct.BrandId = product.BrandId;
                    existingProduct.OrderNo = product.OrderNo;

                    // Görselleri kaldır
                    if (cbRemoveImage)
                    {
                        foreach (var img in existingProduct.ProductImages)
                            FileHelper.FileRemover(img.ImageUrl); // Fiziksel dosyayı sil

                        _context.ProductImages.RemoveRange(existingProduct.ProductImages); // Veritabanından sil
                    }

                    // Yeni görseller ekle
                    if (Images != null && Images.Any())
                    {
                        int order = existingProduct.ProductImages.Any() ? existingProduct.ProductImages.Max(i => i.Order) + 1 : 0;

                        foreach (var image in Images)
                        {
                            var imagePath = await FileHelper.FileLoaderAsync(image);
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                existingProduct.ProductImages.Add(new ProductImage
                                {
                                    ImageUrl = imagePath,
                                    Order = order++
                                });
                            }
                        }
                    }

                    // Kategorileri güncelle
                    existingProduct.ProductCategories.Clear();
                    foreach (var catId in CategoryIds)
                    {
                        existingProduct.ProductCategories.Add(new ProductCategory
                        {
                            CategoryId = catId,
                            ProductId = product.Id
                        });
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["Categories"] = new MultiSelectList(_context.Categories, "Id", "Name", CategoryIds);

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> SetDefaultImage([FromBody] SetDefaultImageRequest request)
        {
            if (request.ProductId <= 0 || request.ImageId <= 0)
                return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
                return NotFound();

            // Tüm görsellerde IsDefault = false yap
            foreach (var image in product.ProductImages)
            {
                image.IsDefault = false;
            }

            // İlgili görseli default olarak ayarla
            var selectedImage = product.ProductImages.FirstOrDefault(x => x.Id == request.ImageId);
            if (selectedImage != null)
            {
                selectedImage.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveImageOrder([FromBody] List<int> orderedImageIds)
        {
            if (orderedImageIds == null || !orderedImageIds.Any())
                return BadRequest("Görsel sıralama listesi boş.");

            var images = await _context.ProductImages
                .Where(p => orderedImageIds.Contains(p.Id))
                .ToListAsync();

            foreach (var id in orderedImageIds)
            {
                var img = images.FirstOrDefault(x => x.Id == id);
                if (img != null)
                {
                    img.Order = orderedImageIds.IndexOf(id);
                }
                else
                {
                    // Opsiyonel: ID eşleşmeyen varsa loglayabilirsin
                    Console.WriteLine($"Image ID {id} veritabanında bulunamadı.");
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteImage([FromBody] ImageDeleteRequest request)
        {
            if (string.IsNullOrEmpty(request.ImageUrl) || request.ProductId <= 0)
                return BadRequest();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
                return NotFound();

            var image = product.ProductImages.FirstOrDefault(i => i.ImageUrl == request.ImageUrl);
            if (image == null)
                return NotFound();

            FileHelper.FileRemover(image.ImageUrl); // Fiziksel dosya silme
            _context.ProductImages.Remove(image);   // Veritabanından kaldırma

            await _context.SaveChangesAsync();
            return Ok();
        }



        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Görselleri fiziksel olarak sil
                foreach (var image in product.ProductImages)
                {
                    FileHelper.FileRemover(image.ImageUrl);
                }

                // EF Core ilişkiyi takip ettiği için ProductImages zaten cascade delete olabilir ama garantiye almak için:
                _context.ProductImages.RemoveRange(product.ProductImages);

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
