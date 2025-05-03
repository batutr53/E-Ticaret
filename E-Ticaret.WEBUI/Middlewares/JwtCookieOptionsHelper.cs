namespace E_Ticaret.WEBUI.Middlewares
{
    public static class JwtCookieOptionsHelper
    {
        public static CookieOptions CreateWriteOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(30)
            };
        }

        public static CookieOptions CreateDeleteOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1)
            };
        }
    }

}
