using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TestUmbraco.Middleware
{
    public class ContentSecurityPolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public ContentSecurityPolicyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Добавляем CSP заголовок для разрешения необходимых ресурсов
            var csp = "default-src 'self'; " +
                      "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://www.google.com https://www.gstatic.com https://cdn.jsdelivr.net https://player.vimeo.com https://mc.yandex.ru https://cdnjs.cloudflare.com https://code.jquery.com https://f.vimeocdn.com https://vimeo.com https://api.vimeo.com https://*.vimeo.com https://*.vimeocdn.com; " +
                      "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://f.vimeocdn.com; " +
                      "img-src 'self' data: https: https://i.vimeocdn.com https://f.vimeocdn.com https://*.vimeo.com https://*.vimeocdn.com; " +
                      "font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com https://cdnjs.cloudflare.com https://cdn.jsdelivr.net https://f.vimeocdn.com; " +
                      "connect-src 'self' https://player.vimeo.com https://mc.yandex.ru https://cdn.jsdelivr.net https://www.gstatic.com https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/ https://cdn.jsdelivr.net/npm/imask@7.1.3/dist/ https://f.vimeocdn.com https://i.vimeocdn.com https://vimeo.com https://api.vimeo.com https://*.vimeo.com https://*.vimeocdn.com; " +
                      "frame-src 'self' https://www.google.com https://player.vimeo.com https://vimeo.com https://api.vimeo.com https://*.vimeo.com https://*.vimeocdn.com; " +
                      "frame-ancestors 'self'; " +
                      "upgrade-insecure-requests;";

            context.Response.Headers["Content-Security-Policy"] = csp;
            
            await _next(context);
        }
    }
}