using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using E_Ticaret.Core.Entities;
using E_Ticaret.WEBUI.Areas.Admin.Controllers;
using E_Ticaret.WEBUI.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace E_Ticaret.Tests;

public class OrderCancellationTests
{
    private sealed class Stub : IPayTrService
    {
        public int Calls;
        public decimal Amount;
        public string? Oid;
        public PayTrRefundResult Result = new(true, null, null);
        public Func<Task>? DuringRefund;
        public async Task<PayTrRefundResult> RefundAsync(string merchantOid, decimal amount, CancellationToken cancellationToken = default)
        {
            Calls++; Amount = amount; Oid = merchantOid;
            if (DuringRefund != null) await DuringRefund();
            return Result;
        }
        public bool ValidateCallback(PayTrCallbackModel callback) => throw new NotSupportedException();
        public string GetPaymentUrl(string token) => throw new NotSupportedException();
        public Task<PayTrTokenResult> CreatePaymentTokenAsync(Order order, string userIp, string successUrl, string failureUrl, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
    private static OrdersController Controller(TestDatabase db, Stub stub) => new(db, stub)
    {
        TempData = new TempDataDictionary(new DefaultHttpContext(), new TempDataProvider())
    };
    private static async Task<Order> Seed(TestDatabase db, OrderStatus status = OrderStatus.Approved)
    {
        var order = new Order { Oid = "TEST123", SubTotal = 100.50m, DeliveryFree = 25.25m,
            OrderStatus = status, OrderDate = DateTime.UtcNow.AddDays(-3) };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return order;
    }

    [Theory]
    [InlineData("Cancel")]
    [InlineData("Refund")]
    [InlineData("UpdateStatus")]
    [InlineData("Edit")]
    public async Task EveryAdminEntryCallsPayTrWithFullStoredAmount(string entry)
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = await Seed(db);
        var stub = new Stub();
        var controller = Controller(db, stub);
        switch (entry)
        {
            case "Cancel": await controller.Cancel(order.Id, default); break;
            case "Refund": await controller.Refund(order.Id, default); break;
            case "UpdateStatus": await controller.UpdateStatus(order.Id, "Cancel"); break;
            case "Edit": order.OrderStatus = OrderStatus.Cancel; await controller.Edit(order.Id, order); break;
        }
        Assert.Equal(1, stub.Calls);
        Assert.Equal(125.75m, stub.Amount);
        Assert.Equal("TEST123", stub.Oid);
        var expected = entry == "Refund" ? OrderStatus.Refund : OrderStatus.Cancel;
        Assert.Equal(expected, (await db.Orders.AsNoTracking().SingleAsync()).OrderStatus);
        await controller.Cancel(order.Id, default);
        await controller.Refund(order.Id, default);
        Assert.Equal(1, stub.Calls);
    }

    [Fact]
    public async Task ConcurrentRequestCannotSendSecondRefund()
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = await Seed(db);
        var stub = new Stub();
        var service = new OrderStatusService(db, stub);
        stub.DuringRefund = async () =>
        {
            Assert.Equal(OrderStatus.RefundProcessing, (await db.Orders.AsNoTracking().SingleAsync()).OrderStatus);
            await service.ChangeAsync(order.Id, OrderStatus.Refund);
        };
        await service.ChangeAsync(order.Id, OrderStatus.Cancel);
        Assert.Equal(1, stub.Calls);
    }

    [Theory]
    [InlineData(false, OrderStatus.Approved)]
    [InlineData(true, OrderStatus.RefundProcessing)]
    public async Task RejectionPreservesStatusAndUncertainOutcomeBlocksRetry(bool uncertain, OrderStatus expected)
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = await Seed(db);
        var stub = new Stub { Result = new(false, "006", "test", uncertain) };
        await new OrderStatusService(db, stub).ChangeAsync(order.Id, OrderStatus.Cancel);
        Assert.Equal(expected, (await db.Orders.AsNoTracking().SingleAsync()).OrderStatus);
        if (uncertain)
        {
            await Controller(db, stub).UpdateStatus(order.Id, "Approved");
            await Controller(db, stub).Cancel(order.Id, default);
            await Controller(db, stub).DeleteConfirmed(order.Id);
            Assert.Equal(1, stub.Calls);
            Assert.Equal(expected, (await db.Orders.AsNoTracking().SingleAsync()).OrderStatus);
        }
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.PaymentFailed)]
    [InlineData(OrderStatus.Cancel)]
    [InlineData(OrderStatus.Refund)]
    public async Task UnpaidAndTerminalOrdersCannotTriggerRefundOrBecomePaidManually(OrderStatus status)
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = await Seed(db, status);
        var stub = new Stub();
        var controller = Controller(db, stub);
        await controller.Cancel(order.Id, default);
        await controller.UpdateStatus(order.Id, "Ready");
        await controller.UpdateStatus(order.Id, "999");
        Assert.Equal(0, stub.Calls);
        Assert.Equal(status, (await db.Orders.AsNoTracking().SingleAsync()).OrderStatus);
    }

    [Fact]
    public async Task EditCannotOverwritePaymentIdentityOrAmount()
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = await Seed(db);
        order.Oid = "OTHER"; order.SubTotal = 1m; order.OrderStatus = OrderStatus.Cancel;
        var stub = new Stub();
        Assert.IsType<ViewResult>(await Controller(db, stub).Edit(order.Id, order));
        Assert.Equal(0, stub.Calls);
        var saved = await db.Orders.AsNoTracking().SingleAsync();
        Assert.Equal("TEST123", saved.Oid);
        Assert.Equal(100.50m, saved.SubTotal);
        Assert.Equal(OrderStatus.Approved, saved.OrderStatus);
    }

    private sealed class Handler(string response, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public string? Body;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("https://www.paytr.com/odeme/iade", request.RequestUri!.ToString());
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new(status) { Content = new StringContent(response) };
        }
    }

    [Fact]
    public async Task RefundUsesOfficialSignatureAndDecimalLiraUnderTurkishCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            var handler = new Handler("{\"status\":\"success\",\"merchant_oid\":\"TEST123\",\"return_amount\":\"125.75\"}");
            var service = new PayTrService(new HttpClient(handler), Options.Create(new PayTrOptions { MerchantId = "123", MerchantKey = "key", MerchantSalt = "salt" }));
            var result = await service.RefundAsync("TEST123", 125.75m);
            Assert.True(result.IsSuccess);
            Assert.Contains("return_amount=125.75", handler.Body);
            var expected = Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes("key"), Encoding.UTF8.GetBytes("123TEST123125.75salt")));
            Assert.Contains("paytr_token=" + Uri.EscapeDataString(expected), handler.Body);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("invalid", 200)]
    [InlineData("{\"status\":\"success\",\"merchant_oid\":\"OTHER\",\"return_amount\":\"125.75\"}", 200)]
    [InlineData("{}", 200)]
    [InlineData("error", 500)]
    public async Task UnverifiableResponsesAreUncertain(string response, int status)
    {
        var handler = new Handler(response, (HttpStatusCode)status);
        var service = new PayTrService(new HttpClient(handler), Options.Create(new PayTrOptions { MerchantId = "123", MerchantKey = "key", MerchantSalt = "salt" }));
        var result = await service.RefundAsync("TEST123", 125.75m);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsUncertain);
    }
}
