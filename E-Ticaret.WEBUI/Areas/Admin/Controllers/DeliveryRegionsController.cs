using System.Globalization;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class DeliveryRegionsController(DatabaseContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.DeliveryRegions.AsNoTracking().Include(x => x.Neighborhoods).OrderBy(x => x.Name).ToListAsync());

    public async Task<IActionResult> Edit(int id = 0)
    {
        var region = id == 0 ? new DeliveryRegion() : await db.DeliveryRegions.AsNoTracking().Include(x => x.Neighborhoods).SingleOrDefaultAsync(x => x.Id == id);
        if (region == null) return NotFound();
        var model = new DeliveryRegionEditModel { Id = id, Name = region.Name, Fee = region.Fee.ToString("0.00", CultureInfo.InvariantCulture),
            IsActive = region.IsActive, NeighborhoodIds = region.Neighborhoods.Select(x => x.Id).ToArray() };
        return await Form(model);
    }

    [HttpGet]
    public async Task<IActionResult> Districts(int provinceId) => Json(await db.AddressDistricts.AsNoTracking()
        .Where(x => x.ProvinceId == provinceId).OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Neighborhoods(int districtId) => Json(await db.AddressNeighborhoods.AsNoTracking()
        .Where(x => x.DistrictId == districtId).OrderBy(x => x.Name).ThenBy(x => x.LocalityName)
        .Select(x => new { x.Id, Name = x.LocalityName == null ? x.Name : x.Name + " (" + x.LocalityName + ")", x.DeliveryRegionId,
            RegionName = x.DeliveryRegion == null ? null : x.DeliveryRegion.Name }).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken, RequestFormLimits(ValueCountLimit = 10000)]
    public async Task<IActionResult> Save(DeliveryRegionEditModel model)
    {
        model.Name = model.Name.Trim();
        var ids = model.NeighborhoodIds.Distinct().ToArray();
        var validFee = decimal.TryParse(model.Fee?.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var amount) && amount >= 0 && amount <= 1000000 && decimal.Round(amount, 2) == amount;
        if (!validFee) ModelState.AddModelError(nameof(model.Fee), "Geçerli bir ücret girin (örnek: 150,50). En fazla iki ondalık basamak kullanın.");
        if ((model.IsActive && ids.Length == 0) || ids.Length > 5000 || ids.Any(x => x <= 0))
            ModelState.AddModelError(nameof(model.NeighborhoodIds), "Listeden en az bir mahalle seçin. Bir bölgeye en fazla 5000 mahalle bağlanabilir.");
        if (!ModelState.IsValid) return await Form(model);
        if (await db.DeliveryRegions.AnyAsync(x => x.Id != model.Id && x.Name.ToLower() == model.Name.ToLower()))
            ModelState.AddModelError(nameof(model.Name), "Bu bölge etiketi zaten kayıtlı.");
        var selected = await db.AddressNeighborhoods.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.DeliveryRegionId }).ToListAsync();
        if (selected.Count != ids.Length) ModelState.AddModelError(nameof(model.NeighborhoodIds), "Adres listesindeki geçerli mahalleleri seçin.");
        if (selected.Any(x => x.DeliveryRegionId.HasValue && x.DeliveryRegionId != model.Id))
            ModelState.AddModelError(nameof(model.NeighborhoodIds), "Seçilen mahallelerden biri başka bir ücret bölgesine bağlı. Önce o bölgeden çıkarın.");
        if (!ModelState.IsValid) return await Form(model);
        var originalId = model.Id;
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var region = model.Id == 0 ? new DeliveryRegion() : await db.DeliveryRegions.FindAsync(model.Id);
            if (region == null) return NotFound();
            region.Name = model.Name;
            region.Fee = amount;
            region.IsActive = model.IsActive;
            if (model.Id == 0) db.DeliveryRegions.Add(region);
            await db.SaveChangesAsync();
            var assigned = await db.AddressNeighborhoods.Where(x => ids.Contains(x.Id) && (x.DeliveryRegionId == null || x.DeliveryRegionId == region.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeliveryRegionId, (int?)region.Id));
            if (assigned != ids.Length) throw new DbUpdateConcurrencyException();
            await db.AddressNeighborhoods.Where(x => x.DeliveryRegionId == region.Id && !ids.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeliveryRegionId, (int?)null));
            await transaction.CommitAsync();
            TempData["RegionSuccess"] = "Teslimat bölgesi ve mahalle eşleştirmeleri kaydedildi.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            db.ChangeTracker.Clear();
            model.Id = originalId;
            ModelState.AddModelError("", "Kayıt başka bir işlemle değişti veya bölge etiketi zaten kullanılıyor. Seçimlerinizi kontrol ederek tekrar deneyin.");
            return await Form(model);
        }
    }

    private async Task<IActionResult> Form(DeliveryRegionEditModel model)
    {
        model.Provinces = await db.AddressProvinces.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        model.SelectedNeighborhoods = await db.AddressNeighborhoods.AsNoTracking().Where(x => model.NeighborhoodIds.Contains(x.Id))
            .OrderBy(x => x.District.Province.Name).ThenBy(x => x.District.Name).ThenBy(x => x.Name)
            .Select(x => new NeighborhoodSelection(x.Id, x.District.Province.Name + " / " + x.District.Name + " / " +
                (x.LocalityName == null ? x.Name : x.Name + " (" + x.LocalityName + ")"), x.District.ProvinceId, x.DistrictId)).ToListAsync();
        return View("Edit", model);
    }
}
