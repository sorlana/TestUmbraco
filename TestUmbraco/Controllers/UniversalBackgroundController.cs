// Controllers/UniversalBackgroundController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace TestUmbraco.Controllers
{
    [Route("api/background")]
    [ApiController]
    public class UniversalBackgroundController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UniversalBackgroundController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public UniversalBackgroundController(
            IMemoryCache memoryCache,
            ILogger<UniversalBackgroundController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            
            _logger.LogInformation("UniversalBackgroundController initialized");
        }

        [HttpGet("multiple")]
        public IActionResult GetMultipleCss([FromQuery] string backgrounds)
        {
            _logger.LogInformation("=== GET /api/background/multiple STARTED ===");
            _logger.LogInformation("Request URL: {Scheme}://{Host}{Path}{QueryString}", 
                HttpContext.Request.Scheme, 
                HttpContext.Request.Host, 
                HttpContext.Request.Path, 
                HttpContext.Request.QueryString);
            _logger.LogInformation("Raw backgrounds parameter: '{Backgrounds}'", backgrounds);

            if (string.IsNullOrEmpty(backgrounds))
            {
                _logger.LogWarning("Backgrounds parameter is empty or null");
                return Content("/* No backgrounds specified */", "text/css");
            }

            var cacheKey = $"BgCss_Multiple_{backgrounds.GetHashCode()}";
            _logger.LogInformation("Cache key: {CacheKey}", cacheKey);
            
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedCss))
            {
                _logger.LogInformation("Cache HIT for key: {CacheKey}", cacheKey);
                _logger.LogInformation("Returning cached CSS of length: {Length}", cachedCss?.Length ?? 0);
                return Content(cachedCss ?? string.Empty, "text/css");
            }
            
            _logger.LogInformation("Cache MISS for key: {CacheKey}", cacheKey);

            var cssBuilder = new StringBuilder();
            var processedCount = 0;
            var errorCount = 0;
            
            try
            {
                // Формат: guid:class:minHeight:size:position
                var backgroundItems = backgrounds.Split('|', StringSplitOptions.RemoveEmptyEntries);
                _logger.LogInformation("Found {Count} background items in parameter", backgroundItems.Length);
                
                for (int i = 0; i < backgroundItems.Length; i++)
                {
                    var item = backgroundItems[i];
                    _logger.LogInformation("Processing item {Index}: '{Item}'", i, item);
                    
                    var parts = item.Split(':');
                    _logger.LogInformation("Item split into {PartsCount} parts", parts.Length);
                    
                    for (int j = 0; j < parts.Length; j++)
                    {
                        _logger.LogInformation("  Part[{Index}]: '{Value}'", j, parts[j]);
                    }
                    
                    if (parts.Length >= 2)
                    {
                        var mediaGuid = parts[0];
                        var className = parts[1];
                        var minHeight = parts.Length > 2 && int.TryParse(parts[2], out var mh) ? mh : 300;
                        var size = parts.Length > 3 ? parts[3] : "cover";
                        var position = parts.Length > 4 ? parts[4] : "center";
                        
                        _logger.LogInformation("Parsed: MediaGuid='{MediaGuid}', ClassName='{ClassName}', MinHeight={MinHeight}, Size='{Size}', Position='{Position}'",
                            mediaGuid, className, minHeight, size, position);

                        if (Guid.TryParse(mediaGuid, out Guid guid))
                        {
                            _logger.LogInformation("Successfully parsed GUID: {Guid}", guid);
                            var mediaUrl = GetMediaUrl(guid, className);
                            
                            _logger.LogInformation("Generated URL for item {Index}: {Url}", i, mediaUrl);
                            
                            cssBuilder.AppendLine($".{className} {{");
                            cssBuilder.AppendLine($"  background-image: url('{mediaUrl}');");
                            cssBuilder.AppendLine($"  background-size: {size};");
                            cssBuilder.AppendLine($"  background-position: {position};");
                            cssBuilder.AppendLine($"  background-repeat: no-repeat;");
                            cssBuilder.AppendLine($"  min-height: {minHeight}px;");
                            cssBuilder.AppendLine("}");
                            
                            if (i < backgroundItems.Length - 1)
                            {
                                cssBuilder.AppendLine(); // Пустая строка между классами
                            }
                            
                            processedCount++;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to parse GUID from: '{MediaGuid}'", mediaGuid);
                            errorCount++;
                            
                            // Все равно генерируем CSS с тестовым изображением
                            var fallbackUrl = GetMediaUrl(Guid.Empty, className);
                            cssBuilder.AppendLine($".{className} {{");
                            cssBuilder.AppendLine($"  background-image: url('{fallbackUrl}');");
                            cssBuilder.AppendLine($"  background-size: {size};");
                            cssBuilder.AppendLine($"  background-position: {position};");
                            cssBuilder.AppendLine($"  background-repeat: no-repeat;");
                            cssBuilder.AppendLine($"  min-height: {minHeight}px;");
                            cssBuilder.AppendLine("}");
                            
                            if (i < backgroundItems.Length - 1)
                            {
                                cssBuilder.AppendLine();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Item has insufficient parts: {PartsCount}, expected at least 2", parts.Length);
                        errorCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating background CSS");
                
                // Все равно возвращаем минимальный CSS для отладки
                cssBuilder.AppendLine("/* ERROR GENERATING CSS */");
                cssBuilder.AppendLine("/* " + ex.Message + " */");
                cssBuilder.AppendLine("/* Stack trace: " + ex.StackTrace + " */");
                
                // Добавляем тестовый класс для отладки
                cssBuilder.AppendLine(".css-debug-error {");
                cssBuilder.AppendLine("  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTkyMCIgaGVpZ2h0PSIxMDgwIiB2aWV3Qm94PSIwIDAgMTkyMCAxMDgwIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9IiNmZjAwMDAiPjwvcmVjdD48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjQ4IiBmaWxsPSJ3aGl0ZSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkVSUk9SPC90ZXh0Pjwvc3ZnPg==');");
                cssBuilder.AppendLine("  background-size: cover;");
                cssBuilder.AppendLine("  background-position: center;");
                cssBuilder.AppendLine("  background-repeat: no-repeat;");
                cssBuilder.AppendLine("  min-height: 300px;");
                cssBuilder.AppendLine("  border: 5px solid red;");
                cssBuilder.AppendLine("}");
                
                _memoryCache.Set(cacheKey, cssBuilder.ToString(), TimeSpan.FromMinutes(5));
                return Content(cssBuilder.ToString(), "text/css");
            }

            var css = cssBuilder.ToString();
            _logger.LogInformation("Generated CSS of length: {Length} characters", css.Length);
            _logger.LogInformation("Successfully processed: {ProcessedCount} items, errors: {ErrorCount}", processedCount, errorCount);
            
            if (string.IsNullOrEmpty(css))
            {
                _logger.LogWarning("Generated CSS is empty");
                css = "/* No valid backgrounds found */";
                
                // Добавляем тестовый класс для отладки
                css += "\n.css-debug-empty {";
                css += "\n  background-image: url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTkyMCIgaGVpZ2h0PSIxMDgwIiB2aWV3Qm94PSIwIDAgMTkyMCAxMDgwIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9IiNmZmZmMDAiPjwvcmVjdD48dGV4dCB4PSI1MCUiIHk9IjUwJSIgZm9udC1mYW1pbHk9IkFyaWFsIiBmb250LXNpemU9IjQ4IiBmaWxsPSJibGFjayIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkVNUFRZPC90ZXh0Pjwvc3ZnPg==');";
                css += "\n  background-size: cover;";
                css += "\n  background-position: center;";
                css += "\n  background-repeat: no-repeat;";
                css += "\n  min-height: 300px;";
                css += "\n  border: 5px solid yellow;";
                css += "\n}";
            }
            else
            {
                // Добавляем отладочный комментарий в начало CSS
                css = $"/* Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss} */\n" +
                      $"/* Items: {processedCount}, Errors: {errorCount} */\n" +
                      css;
            }

            _logger.LogInformation("Setting cache for key: {CacheKey} with TTL: 1 hour", cacheKey);
            _memoryCache.Set(cacheKey, css, TimeSpan.FromHours(1));
            
            _logger.LogInformation("=== GET /api/background/multiple COMPLETED ===");
            
            // Добавляем заголовки для отладки
            Response.Headers.Append("X-Backgrounds-Count", processedCount.ToString());
            Response.Headers.Append("X-Backgrounds-Errors", errorCount.ToString());
            Response.Headers.Append("X-Cache-Status", "MISS");
            
            return Content(css, "text/css");
        }

        private string GetMediaUrl(Guid mediaGuid, string className)
        {
            _logger.LogInformation("GetMediaUrl called with Guid: {Guid}, ClassName: {ClassName}", mediaGuid, className);
            
            // Создаем SVG изображение в виде Data URL
            var colors = new[]
            {
                "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", 
                "#00BCD4", "#8BC34A", "#FF5722", "#607D8B"
            };
            
            var colorIndex = Math.Abs(mediaGuid.GetHashCode()) % colors.Length;
            var color = colors[colorIndex];
            
            // SVG с градиентом и текстом
            var svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='1920' height='1080'>
                <defs>
                    <linearGradient id='grad' x1='0%' y1='0%' x2='100%' y2='100%'>
                        <stop offset='0%' style='stop-color:{color};stop-opacity:0.8' />
                        <stop offset='100%' style='stop-color:{DarkenColor(color, 20)};stop-opacity:1' />
                    </linearGradient>
                </defs>
                <rect width='100%' height='100%' fill='url(#grad)'/>
                <text x='50%' y='50%' font-family='Arial' font-size='48' fill='white' 
                      text-anchor='middle' dy='.3em' opacity='0.7'>{className}</text>
            </svg>";
            
            var svgDataUrl = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
            
            _logger.LogInformation("Generated SVG Data URL for class: {ClassName}", className);
            return svgDataUrl;
        }

        private string DarkenColor(string hexColor, int percent)
        {
            // Простая функция для затемнения цвета
            var color = System.Drawing.ColorTranslator.FromHtml(hexColor);
            
            var factor = (100 - percent) / 100f;
            var r = (int)(color.R * factor);
            var g = (int)(color.G * factor);
            var b = (int)(color.B * factor);
            
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        
        // Метод для прямой отладки
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("=== TEST ENDPOINT CALLED ===");
            
            var testBackgrounds = new List<string>
            {
                "00000000-0000-0000-0000-000000000001:test-bg-1:500:cover:center",
                "00000000-0000-0000-0000-000000000002:test-bg-2:600:contain:top",
                "invalid-guid:test-bg-3:700:auto:left"
            };
            
            var testParam = string.Join("|", testBackgrounds);
            
            _logger.LogInformation("Redirecting to /multiple with param: {Param}", testParam);
            
            return Redirect($"/api/background/multiple?backgrounds={Uri.EscapeDataString(testParam)}");
        }
        
        [HttpGet("health")]
        public IActionResult Health()
        {
            _logger.LogInformation("Health check called");
            
            var healthInfo = new
            {
                Status = "OK",
                Timestamp = DateTime.UtcNow,
                Environment = _webHostEnvironment.EnvironmentName,
                ApplicationName = _webHostEnvironment.ApplicationName,
                CacheEnabled = _memoryCache != null,
                LoggingEnabled = _logger != null,
                HttpContextAccessorAvailable = _httpContextAccessor != null
            };
            
            return Ok(healthInfo);
        }
    }
}