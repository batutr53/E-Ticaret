using System.Text;
using E_Ticaret.WEBUI.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace E_Ticaret.Tests;

public class AdminErrorResponseTests
{
    [Fact]
    public async Task AdminErrorResponseDeclaresUtf8AndPreservesTurkishText()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/Admin/DeliveryRegions";
        context.Response.Body = new MemoryStream();
        var middleware = new AdminExceptionMiddleware(_ => throw new InvalidOperationException("internal details"),
            NullLogger<AdminExceptionMiddleware>.Instance);
        await middleware.Invoke(context);
        var body = Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", context.Response.ContentType);
        Assert.Contains("<meta charset='utf-8'>", body);
        Assert.Contains("Yönetici Panelinde Bir Hata Oluştu", body);
        Assert.Contains("Lütfen daha sonra tekrar deneyin", body);
        Assert.DoesNotContain("internal details", body);
    }
}
