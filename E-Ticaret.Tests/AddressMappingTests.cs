using System.Text.Json;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data.ReferenceData;
using E_Ticaret.WEBUI.Areas.Admin.Controllers;
using E_Ticaret.WEBUI.Areas.Admin.Models;
using E_Ticaret.WEBUI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Tests;

public class AddressMappingTests
{
    [Fact]
    public void ReferenceCatalogRetainsUniqueSourceIdsAndValidHierarchy()
    {
        var data = AddressReferenceCatalog.Load();
        Assert.Equal(81, data.Provinces.Count);
        Assert.Equal(973, data.Districts.Count);
        Assert.Equal(32279, data.Neighborhoods.Count);
        Assert.Equal(data.Neighborhoods.Count, data.Neighborhoods.Select(x => x.Id).Distinct().Count());
        var provinces = data.Provinces.Select(x => x.Id).ToHashSet();
        var districts = data.Districts.Select(x => x.Id).ToHashSet();
        Assert.All(data.Districts, x => Assert.Contains(x.ProvinceId, provinces));
        Assert.All(data.Neighborhoods, x => Assert.Contains(x.DistrictId, districts));
        Assert.Equal("İstanbul", data.Provinces.Single(x => x.Id == 34).Name);
        Assert.Equal("Ümraniye", data.Districts.Single(x => x.Id == 1852).Name);
        Assert.Equal("Çamlık", data.Neighborhoods.Single(x => x.Id == 98689).Name);
        Assert.All(data.Neighborhoods, x => Assert.InRange(x.Name.Length, 1, 160));
    }

    [Fact]
    public async Task CustomerListsOnlyMappedActiveNeighborhoodsAndTheirDatabaseFees()
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedAddressAsync();
        db.AddressNeighborhoods.AddRange(new AddressNeighborhood { Id = 98690, Name = "Tanımsız", DistrictId = 1852 },
            new AddressNeighborhood { Id = 98691, Name = "Kapalı", DistrictId = 1852,
                DeliveryRegion = new DeliveryRegion { Name = "Kapalı bölge", Fee = 0, IsActive = false } });
        await db.SaveChangesAsync();
        var controller = new DeliveryAddressController(db);
        var result = Assert.IsType<JsonResult>(await controller.Neighborhoods(1852, default));
        var rows = JsonSerializer.SerializeToElement(result.Value);
        Assert.Equal(1, rows.GetArrayLength());
        Assert.Equal(98689, rows[0].GetProperty("Id").GetInt32());
        Assert.Equal(150.25m, rows[0].GetProperty("Fee").GetDecimal());
        var missingDistrict = Assert.IsType<JsonResult>(await controller.Districts(6, default));
        Assert.Equal(0, JsonSerializer.SerializeToElement(missingDistrict.Value).GetArrayLength());
    }

    [Theory]
    [InlineData(98689)]
    [InlineData(999999)]
    public async Task AdminCannotClaimAnotherRegionsNeighborhoodOrAnUnknownId(int neighborhoodId)
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedAddressAsync();
        var controller = new DeliveryRegionsController(db);
        var result = await controller.Save(new DeliveryRegionEditModel { Name = "Başka bölge", Fee = "0", NeighborhoodIds = new[] { neighborhoodId } });
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, await db.DeliveryRegions.CountAsync());
        Assert.Equal(1, (await db.AddressNeighborhoods.SingleAsync()).DeliveryRegionId);
    }

    [Fact]
    public async Task AdminCanReleaseTheLastNeighborhoodFromAnInactiveRegion()
    {
        await using var db = await TestDatabase.CreateAsync();
        var address = await db.SeedAddressAsync();
        var controller = new DeliveryRegionsController(db)
        { TempData = new TempDataDictionary(new DefaultHttpContext(), new EmptyTempDataProvider()) };
        var result = await controller.Save(new DeliveryRegionEditModel { Id = address.DeliveryRegionId!.Value,
            Name = "Yakın teslimat", Fee = "0", IsActive = false });
        Assert.IsType<RedirectToActionResult>(result);
        db.ChangeTracker.Clear();
        Assert.Null((await db.AddressNeighborhoods.SingleAsync()).DeliveryRegionId);
    }

    [Fact]
    public async Task ActiveRegionRequiresAtLeastOneCatalogNeighborhood()
    {
        await using var db = await TestDatabase.CreateAsync();
        var controller = new DeliveryRegionsController(db);
        Assert.IsType<ViewResult>(await controller.Save(new DeliveryRegionEditModel { Name = "Boş bölge", Fee = "0", IsActive = true }));
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(await db.DeliveryRegions.ToListAsync());
    }

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
