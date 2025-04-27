namespace E_Ticaret.WEBUI.ExtensionMethods
{
    using Microsoft.AspNetCore.Http;

    public static class HttpRequestExtensions
    {
        public static string GetBaseUrl(this HttpRequest request, bool includePort = false)
        {
            var scheme = request.Scheme; // http veya https
            var host = request.Host.Host; // sadece domain (localhost, example.com gibi)
            var port = request.Host.Port; // port numarası (int?)

            if (includePort && port.HasValue)
            {
                return $"{scheme}://{host}:{port}";
            }
            else
            {
                return $"{scheme}://{host}";
            }
        }
    }

}
