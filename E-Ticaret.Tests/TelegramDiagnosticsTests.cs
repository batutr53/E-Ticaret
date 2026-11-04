using System.Net;
using E_Ticaret.WEBUI.Helpers;
using Microsoft.Extensions.Configuration;

namespace E_Ticaret.Tests;

public class TelegramDiagnosticsTests
{
    private sealed class Handler(string response, HttpStatusCode code) : HttpMessageHandler
    {
        public List<string> Methods = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Methods.Add(request.RequestUri!.Segments.Last());
            return Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(response) });
        }
    }
    private static TelegramHelper Helper(Handler handler) => new(new HttpClient(handler), new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["TelegramBot:BotToken"] = "SECRET", ["TelegramBot:ChatId"] = "test"
    }).Build());

    [Fact]
    public async Task InvalidTokenHasActionableDiagnosisWithoutLeakingApiResponse()
    {
        var handler = new Handler("{\"ok\":false,\"error_code\":401,\"description\":\"SECRET\"}", HttpStatusCode.Unauthorized);
        var result = await Helper(handler).CheckConnectionAsync();
        Assert.Contains("401", result);
        Assert.Contains("BotFather", result);
        Assert.DoesNotContain("SECRET", result);
        Assert.Equal(new[] { "getMe" }, handler.Methods);
    }

    [Fact]
    public async Task ConnectionCheckDoesNotSendMessages()
    {
        var handler = new Handler("{\"ok\":true,\"result\":{}}", HttpStatusCode.OK);
        Assert.Contains("test mesajı gönderilmedi", await Helper(handler).CheckConnectionAsync());
        Assert.Equal(new[] { "getMe", "getChat" }, handler.Methods);
    }

    [Theory]
    [InlineData(401, "Unauthorized", "BotFather")]
    [InlineData(403, "Forbidden", "engellenmiş")]
    [InlineData(400, "Bad Request: chat not found", "Sohbet numarasını")]
    public async Task SendFailureReportsSafeSpecificReason(int code, string description, string expected)
    {
        var handler = new Handler(System.Text.Json.JsonSerializer.Serialize(new { ok = false, error_code = code, description }), (HttpStatusCode)code);
        var error = await Assert.ThrowsAsync<TelegramDeliveryException>(() => Helper(handler).SendTelegramMessage("test"));
        Assert.Contains(expected, error.Message);
        Assert.DoesNotContain("SECRET", error.ToString());
    }
}
