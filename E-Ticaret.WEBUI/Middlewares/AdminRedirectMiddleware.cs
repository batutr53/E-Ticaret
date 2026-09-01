public class AdminRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public AdminRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Response'u geçici olarak buffer'la
        var originalBodyStream = context.Response.Body;
        using (var memStream = new MemoryStream())
        {
            context.Response.Body = memStream;
            try
            {
                await _next(context);

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    var path = context.Request.Path.ToString().ToLower();
                    if (path.StartsWith("/admin") && !path.Contains("/admin/auth/login"))
                    {
                        context.Response.Clear();
                        context.Response.Redirect("/Admin/Auth/Login");
                    }
                }

                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
