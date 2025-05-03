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
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .ToListAsync();

            return View(products);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create(Product product, IFormFile? Image, List<int> CategoryIds)
        {
            if (ModelState.IsValid)
            {
                product.Image = await FileHelper.FileLoaderAsync(Image);

                // Kategori ilişkilerini kur
                product.ProductCategories = CategoryIds.Select(id => new ProductCategory
                {
                    CategoryId = id
                }).ToList();

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
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);

            // Seçili kategori ID'lerini çıkar
            var selectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();
            ViewData["Categories"] = new MultiSelectList(_context.Categories, "Id", "Name", selectedCategoryIds);

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? Image, List<int> CategoryIds, bool cbRemoveImage = false)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanındaki mevcut product'u takip et
                    var existingProduct = await _context.Products
                        .Include(p => p.ProductCategories)
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

                    // Görsel silme veya değiştirme
                    if (cbRemoveImage)
                        existingProduct.Image = string.Empty;
                    else if (Image is not null)
                        existingProduct.Image = await FileHelper.FileLoaderAsync(Image);

                    // Kategori ilişkilerini güncelle
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


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }


        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.Image))
                {
                    FileHelper.FileRemover(product.Image);
                }
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
