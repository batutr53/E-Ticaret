using System.Net;
using System.Text;
using System.Text.Json;
using E_Ticaret.Core.Entities;
using E_Ticaret.WEBUI.Payments;
using Microsoft.Extensions.Options;

namespace E_Ticaret.Tests;

public class PaymentAmountTests
{
    private sealed class Handler : HttpMessageHandler
    {
        public Dictionary<string, string> Form { get; private set; } = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Form = (await request.Content!.ReadAsStringAsync(cancellationToken)).Split('&')
                .Select(x => x.Split('=', 2)).ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x[1]));
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"status\":\"success\",\"token\":\"test-token\"}") };
        }
    }

    [Fact]
    public async Task PayTrReceivesDeliveryFeeInBothAmountAndBasket()
    {
        var handler = new Handler();
        var service = new PayTrService(new HttpClient(handler), Options.Create(new PayTrOptions { MerchantId = "1", MerchantKey = "test", MerchantSalt = "test" }));
        var order = new Order { Oid = "test", SubTotal = 2001m, DeliveryFree = 175.50m,
            OrderItems = new List<OrderItem> { new() { ProductName = "Çiçek", Price = 1000.50m, Quantity = 2 } } };
        Assert.True((await service.CreatePaymentTokenAsync(order, "127.0.0.1", "https://example.com/ok", "https://example.com/fail")).IsSuccess);
        Assert.Equal("217650", handler.Form["payment_amount"]);
        using var basket = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(handler.Form["user_basket"])));
        Assert.Equal(2, basket.RootElement.GetArrayLength());
        Assert.Equal("175.50", basket.RootElement[1][1].GetString());
    }
}
