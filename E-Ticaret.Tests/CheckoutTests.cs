using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Concrete;
using E_Ticaret.WEBUI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Tests;

public class CheckoutTests
{
    private static CreateOrderDTO Input(int? neighborhoodId = 98689, string fee = "150.25") => new()
    {
        CustomerId = "test-customer", DeliveryRegionId = 999, DeliveryFree = -999, City = "Yanlış il",
        ProvinceId = 34, DistrictId = 1852, NeighborhoodId = neighborhoodId, QuotedDeliveryFee = fee,
        Description = "Kapıda arayın", CardMessage = "İyi ki doğdun!", Oid = Guid.NewGuid().ToString("N")
    };

    [Fact]
    public async Task CalculatesFeeOnServerAndPreservesHistoricalSnapshot()
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        var neighborhood = await db.SeedAddressAsync();
        var region = neighborhood.DeliveryRegion!;
        var order = await new OrderService(db).CreateOrder(Input());
        Assert.Equal(2001m, order.SubTotal);
        Assert.Equal(150.25m, order.DeliveryFree);
        Assert.Equal(2151.25m, order.GetTotal());
        Assert.Equal("İyi ki doğdun!", order.CardMessage);
        Assert.Equal("Kapıda arayın", order.Description);
        region.Fee = 999;
        region.Name = "Değişen bölge";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var saved = await db.Orders.SingleAsync();
        Assert.Equal(150.25m, saved.DeliveryFree);
        Assert.Equal("Yakın teslimat", saved.DeliveryRegionName);
        Assert.Equal("İstanbul", saved.City);
        Assert.Equal(34, saved.DeliveryProvinceId);
        Assert.Equal(1852, saved.DeliveryDistrictId);
        Assert.Equal(98689, saved.DeliveryNeighborhoodId);
        Assert.Equal("Ümraniye", saved.DeliveryDistrictName);
        Assert.Equal("Çamlık", saved.DeliveryNeighborhoodName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(999)]
    [InlineData(98689)]
    public async Task RejectsMissingUnknownOrInactiveNeighborhood(int? id)
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync(active: false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new OrderService(db).CreateOrder(Input(id)));
        Assert.Empty(await db.Orders.ToListAsync());
        Assert.Equal(50, (await db.Products.SingleAsync()).Stock);
    }

    [Fact]
    public async Task UnmappedNeighborhoodCannotCheckoutEvenWithNoRegions()
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync(mapped: false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new OrderService(db).CreateOrder(Input()));
        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task ExplicitlyMappedFreeNeighborhoodIsFree()
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync(fee: 0);
        var order = await new OrderService(db).CreateOrder(Input(fee: "0"));
        Assert.Equal(0, order.DeliveryFree);
        Assert.Equal(2001m, order.GetTotal());
    }

    [Theory]
    [InlineData(6, 1852)]
    [InlineData(34, 1)]
    public async Task RejectsMismatchedProvinceAndDistrict(int provinceId, int districtId)
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync();
        var input = Input(); input.ProvinceId = provinceId; input.DistrictId = districtId;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new OrderService(db).CreateOrder(input));
        Assert.Empty(await db.Orders.ToListAsync());
        Assert.Equal(50, (await db.Products.SingleAsync()).Stock);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("149.99")]
    [InlineData(null)]
    public async Task ChangedOrMissingQuoteCannotChargeAnUnexpectedAmount(string? quote)
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync();
        await db.SeedAddressAsync();
        var input = Input(); input.QuotedDeliveryFee = quote;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new OrderService(db).CreateOrder(input));
        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task InactiveProductCannotBeOrderedOrAddedToCart()
    {
        await using var db = await TestDatabase.CreateAsync();
        var product = await db.SeedCartAsync(false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new OrderService(db).CreateOrder(Input()));
        var cart = new CartController(new Service<Product>(db), new Service<Cart>(db));
        // Reject before reading cookies or creating a cart.
        Assert.IsType<BadRequestObjectResult>(await cart.AddItemToCart(product.Id, -1));
        Assert.Empty(await db.Orders.ToListAsync());
    }

    [Fact]
    public async Task CategoryHidesInactiveProducts()
    {
        await using var db = await TestDatabase.CreateAsync();
        await db.SeedCartAsync(false);
        var controller = new CategoryController(new Service<Category>(db));
        var result = Assert.IsType<ViewResult>(await controller.Index(2, "hediyelik"));
        var category = Assert.IsType<Category>(result.Model);
        Assert.Empty(category.ProductCategories!);
    }

    [Fact]
    public async Task EditingSeoTextPreservesCategoryPhoto()
    {
        await using var db = await TestDatabase.CreateAsync();
        var category = await db.Categories.SingleAsync(x => x.Id == 2);
        category.Image = "existing-photo.jpg";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var controller = new E_Ticaret.WEBUI.Areas.Admin.Controllers.CategoriesController(db, new Service<Category>(db));
        var edited = new Category { Id = 2, Name = "Hediyelik", IsActive = true, Description = "<p>&amp;ccedil;i&amp;ccedil;ek</p>" };
        Assert.IsType<RedirectToActionResult>(await controller.Edit(2, edited, null));
        db.ChangeTracker.Clear();
        var saved = await db.Categories.SingleAsync(x => x.Id == 2);
        Assert.Equal("existing-photo.jpg", saved.Image);
        Assert.Equal("<p>çiçek</p>", saved.Description);
    }
}
