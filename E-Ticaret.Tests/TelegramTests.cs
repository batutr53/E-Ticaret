using System.Net;
using E_Ticaret.Core.Entities;
using E_Ticaret.Core.Email;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using E_Ticaret.WEBUI.Controllers;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace E_Ticaret.Tests;

public class TelegramTests
{
    private static IConfiguration Config() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["TelegramBot:BotToken"] = "test-token", ["TelegramBot:ChatId"] = "test-chat",
        ["TelegramBot:AdminPanelBaseUrl"] = "https://example.com/Admin/Orders/Details/"
    }).Build();

    private sealed class Handler : HttpMessageHandler
    {
        public bool Success { get; set; }
        public int Calls { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.DoesNotContain("chat_id", request.RequestUri!.Query);
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(Success ? HttpStatusCode.OK : HttpStatusCode.Forbidden)
            { Content = new StringContent(Success ? "{\"ok\":true}" : "{\"ok\":false}") };
        }
    }

    [Fact]
    public async Task FailedTelegramDeliveryIsPersistedRetriedAndNotResentAfterSuccess()
    {
        var db = await TestDatabase.CreateAsync();
        var order = new Order { Oid = "test-order", OrderStatus = OrderStatus.Approved };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        db.TelegramNotifications.Add(new TelegramNotification { OrderId = order.Id });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var handler = new Handler();
        var services = new ServiceCollection().AddSingleton<DatabaseContext>(db)
            .AddSingleton(new TelegramHelper(new HttpClient(handler), Config())).BuildServiceProvider();
        await using (services)
        {
            var worker = new TelegramNotificationWorker(services.GetRequiredService<IServiceScopeFactory>(), NullLogger<TelegramNotificationWorker>.Instance);
            await worker.ProcessPendingAsync(default);
            var failed = await db.TelegramNotifications.SingleAsync();
            Assert.Null(failed.SentAt);
            Assert.Equal(1, failed.Attempts);
            Assert.NotNull(failed.LastError);
            Assert.True(failed.NextAttemptAt > DateTime.UtcNow);
            await worker.ProcessPendingAsync(default);
            Assert.Equal(1, handler.Calls);
            failed.NextAttemptAt = DateTime.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            handler.Success = true;
            await worker.ProcessPendingAsync(default);
            var sent = await db.TelegramNotifications.SingleAsync();
            Assert.NotNull(sent.SentAt);
            Assert.Null(sent.LastError);
            Assert.Equal(2, sent.Attempts);
            await worker.ProcessPendingAsync(default);
            Assert.Equal(2, handler.Calls);
        }
    }

    [Fact]
    public async Task TelegramRejectsApiFailureAndEscapesMessageHtml()
    {
        var handler = new Handler();
        var telegram = new TelegramHelper(new HttpClient(handler), Config());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => telegram.SendTelegramOrderMessage(5, "A < B & C"));
        Assert.DoesNotContain("test-token", error.ToString());
        Assert.Contains("A+%26lt%3B+B+%26amp%3B+C", handler.Body);
    }

    [Fact]
    public async Task PaidCallbackQueuesTelegramEvenIfEmailsFailAndRepeatedCallbackDoesNotDuplicate()
    {
        await using var db = await TestDatabase.CreateAsync();
        var order = new Order { Oid = "test-paid", SenderEmail = "customer@example.com", SubTotal = 100m, DeliveryFree = 25m };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        var email = new FailingEmail();
        var controller = new OrderController(new OrderService(db), db, new CartService(db), email,
            Options.Create(new EmailSettings { AdminEmail = "admin@example.com" }), new Renderer(),
            new TelegramHelper(new HttpClient(new Handler()), Config()), new PayTrStub(), Options.Create(new PayTrOptions()),
            NullLogger<OrderController>.Instance);
        var callback = new PayTrCallbackModel { MerchantOid = order.Oid, Status = "success", PaymentAmount = "12500" };
        Assert.Equal("OK", Assert.IsType<ContentResult>(await controller.PayTrCallback(callback, default)).Content);
        db.ChangeTracker.Clear();
        Assert.Equal(OrderStatus.Approved, (await db.Orders.SingleAsync()).OrderStatus);
        Assert.Single(await db.TelegramNotifications.ToListAsync());
        Assert.Equal(2, email.Calls);
        await controller.PayTrCallback(callback, default);
        Assert.Single(await db.TelegramNotifications.ToListAsync());
        Assert.Equal(2, email.Calls);
    }

    private sealed class FailingEmail : IEmailService
    {
        public int Calls { get; private set; }
        public Task SendEmailAsync(string toEmail, string subject, string htmlBody) { Calls++; throw new InvalidOperationException("SMTP unavailable"); }
    }
    private sealed class Renderer : IRazorViewToStringRenderer
    {
        public Task<string> RenderViewToStringAsync(string viewName, object model) => Task.FromResult("<p>Test</p>");
    }
    private sealed class PayTrStub : IPayTrService
    {
        public bool ValidateCallback(PayTrCallbackModel callback) => true;
        public Task<PayTrTokenResult> CreatePaymentTokenAsync(Order order, string userIp, string successUrl, string failureUrl, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PayTrRefundResult> RefundAsync(string merchantOid, decimal amount, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public string GetPaymentUrl(string token) => "https://example.com/payment";
    }
}
