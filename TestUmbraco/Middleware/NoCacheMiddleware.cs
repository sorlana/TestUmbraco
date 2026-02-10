using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TestUmbraco.Middleware
{
    public class NoCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public NoCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Отключаем кеш для backgrounds.css
            if (context.Request.Path.StartsWithSegments("/css/backgrounds.css"))
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }

            await _next(context);
        }
    }
}
