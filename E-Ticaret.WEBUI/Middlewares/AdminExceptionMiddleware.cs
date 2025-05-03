namespace E_Ticaret.WEBUI.Middlewares
{
    public class AdminExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminExceptionMiddleware> _logger;

        public AdminExceptionMiddleware(RequestDelegate next, ILogger<AdminExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Request.Path.StartsWithSegments("/admin"))
                {
                    _logger.LogError(ex, "Admin panelinde hata oluştu: {Path}", context.Request.Path);

                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";

                    // Basit HTML mesajı (istersen Razor view’a yönlendirme de yapabiliriz)
                    await context.Response.WriteAsync(@"
                    <html>
                        <head><title>Hata</title></head>
                        <body style='font-family:sans-serif; padding: 40px;'>
                            <h2>Yönetici Panelinde Bir Hata Oluştu</h2>
                            <p>Lütfen daha sonra tekrar deneyin veya teknik destekle iletişime geçin.</p>
                        </body>
                    </html>
                ");
                }
                else
                {
                    // Admin değilse hata dışarı çıksın (global hata handler varsa o alır)
                    throw;
                }
            }
        }
    }

}
