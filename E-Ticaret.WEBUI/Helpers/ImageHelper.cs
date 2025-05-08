namespace E_Ticaret.WEBUI.Helpers
{
    public static class ImageHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string ToFullImageUrl(string relativeImagePath)
        {
            if (string.IsNullOrWhiteSpace(relativeImagePath))
                relativeImagePath = "/img/no-image.png";

            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request == null)
                return relativeImagePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{(relativeImagePath.StartsWith("/") ? "" : "/")}{relativeImagePath}";
        }
    }
}
