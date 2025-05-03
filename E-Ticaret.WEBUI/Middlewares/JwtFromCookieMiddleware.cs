namespace E_Ticaret.WEBUI.Middlewares
{
    public class JwtFromCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtFromCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    if (token == "logout") return; // token logout ile overwrite edildiyse skip
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                }
            }

            await _next(context);
        }
    }

}
