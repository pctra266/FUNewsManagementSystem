namespace PhamCongTra_SE1885NET_A01_FE.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip authentication for login page and public assets
            if (path == "/login" || path?.StartsWith("/css") == true ||
                path?.StartsWith("/js") == true || path?.StartsWith("/lib") == true)
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            var token = context.Session.GetString("AuthToken");

            if (string.IsNullOrEmpty(token))
            {
                context.Response.Redirect("/Login");
                return;
            }

            await _next(context);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}