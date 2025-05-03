namespace E_Ticaret.WEBUI.Middlewares
{
    public class AdminRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();

            if (path.StartsWith("/admin") && !path.Contains("/admin/auth/login"))
            {
                var token = context.Request.Cookies["jwt"];

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.Redirect("/Admin/Auth/Login");
                    return;
                }
            }

            await _next(context);
        }
    }

}
