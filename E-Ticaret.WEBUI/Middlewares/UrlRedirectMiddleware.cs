using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Middlewares
{
    public class UrlRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UrlRedirectMiddleware> _logger;

        public UrlRedirectMiddleware(RequestDelegate next, ILogger<UrlRedirectMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var query = context.Request.QueryString.Value ?? string.Empty;
            var host = context.Request.Host.Value;
            var isGet = HttpMethods.IsGet(context.Request.Method);

            // Yönlendirme yapılacak mı?
            var shouldRedirect = false;
            var redirectUrl = path + query;

            // 1. Trailing slash kontrolü
            if (path.Length > 1 && path.EndsWith('/'))
            {
                redirectUrl = path.TrimEnd('/') + query;
                shouldRedirect = true;
            }
            // 2. Büyük harf kontrolü
            else if (path != path.ToLowerInvariant() && path.IndexOf("/api/", StringComparison.OrdinalIgnoreCase) == -1)
            {
                redirectUrl = path.ToLowerInvariant() + query;
                shouldRedirect = true;
            }
            // 3. URL'de Türkçe karakter kontrolü
            else if (ContainsTurkishCharacters(path))
            {
                redirectUrl = path.ToLowerInvariant()
                    .Replace('ü', 'u')
                    .Replace('ı', 'i')
                    .Replace('ö', 'o')
                    .Replace('ç', 'c')
                    .Replace('ğ', 'g')
                    .Replace('ş', 's') + query;
                shouldRedirect = true;
            }

            // Yönlendirme gerekliyse 301 (kalıcı yönlendirme) yap
            if (shouldRedirect && isGet)
            {
                _logger.LogInformation($"Redirecting: {path} -> {redirectUrl}");
                context.Response.Redirect(redirectUrl, true);
                return;
            }

            await _next(context);
        }

        private bool ContainsTurkishCharacters(string text)
        {
            return text.IndexOf('ü') >= 0 ||
                   text.IndexOf('ı') >= 0 ||
                   text.IndexOf('ö') >= 0 ||
                   text.IndexOf('ç') >= 0 ||
                   text.IndexOf('ğ') >= 0 ||
                   text.IndexOf('ş') >= 0 ||
                   text.IndexOf('Ü') >= 0 ||
                   text.IndexOf('İ') >= 0 ||
                   text.IndexOf('Ö') >= 0 ||
                   text.IndexOf('Ç') >= 0 ||
                   text.IndexOf('Ğ') >= 0 ||
                   text.IndexOf('Ş') >= 0;
        }
    }
}
