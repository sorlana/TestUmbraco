// Controllers/UniversalBackgroundController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace TestUmbraco.Controllers
{
    [Route("api/background")]
    [ApiController]
    public class UniversalBackgroundController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UniversalBackgroundController> _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMediaService _mediaService;

        public UniversalBackgroundController(
            IMemoryCache memoryCache,
            ILogger<UniversalBackgroundController> logger,
            IUmbracoContextAccessor umbracoContextAccessor,
            IMediaService mediaService)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _mediaService = mediaService;
        }

        [HttpGet("multiple")]
        public IActionResult GetMultipleCss([FromQuery] string backgrounds)
        {
            if (string.IsNullOrEmpty(backgrounds))
            {
                return Content("/* No backgrounds specified */", "text/css");
            }

            var cacheKey = $"BgCss_Multiple_{backgrounds.GetHashCode()}";
            
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedCss))
            {
                return Content(cachedCss ?? string.Empty, "text/css");
            }

            var cssBuilder = new StringBuilder();
            var processedCount = 0;
            
            try
            {
                var backgroundItems = backgrounds.Split('|', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var item in backgroundItems)
                {
                    var parts = item.Split(':');
                    
                    if (parts.Length >= 2)
                    {
                        var mediaGuidStr = parts[0];
                        var className = parts[1];
                        var minHeight = parts.Length > 2 && int.TryParse(parts[2], out var mh) ? mh : 400;
                        var size = parts.Length > 3 ? parts[3] : "cover";
                        var position = parts.Length > 4 ? parts[4] : "center";

                        if (Guid.TryParse(mediaGuidStr, out Guid mediaGuid) && mediaGuid != Guid.Empty)
                        {
                            var mediaUrl = GetMediaUrl(mediaGuid);
                            
                            cssBuilder.AppendLine($".{className} {{");
                            cssBuilder.AppendLine($"  background-image: url('{mediaUrl}');");
                            cssBuilder.AppendLine($"  background-size: {size};");
                            cssBuilder.AppendLine($"  background-position: {position};");
                            cssBuilder.AppendLine($"  background-repeat: no-repeat;");
                            cssBuilder.AppendLine($"  min-height: {minHeight}px;");
                            cssBuilder.AppendLine("}}");
                            cssBuilder.AppendLine();
                            
                            processedCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating background CSS");
                return Content("/* Error generating CSS */", "text/css");
            }

            var css = cssBuilder.ToString();
            
            if (string.IsNullOrWhiteSpace(css))
            {
                css = "/* No valid backgrounds generated */";
            }
            
            _memoryCache.Set(cacheKey, css, TimeSpan.FromHours(1));
            
            return Content(css, "text/css");
        }

        private string GetMediaUrl(Guid mediaGuid)
        {
            try
            {
                if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                {
                    var mediaItem = umbracoContext.Media?.GetById(mediaGuid);
                    
                    if (mediaItem != null)
                    {
                        return mediaItem.Url();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting media URL");
            }
            
            // Fallback для отладки
            return GenerateFallbackImageUrl(mediaGuid);
        }

        private string GenerateFallbackImageUrl(Guid mediaGuid)
        {
            var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#00BCD4" };
            var colorIndex = Math.Abs(mediaGuid.GetHashCode()) % colors.Length;
            var color = colors[colorIndex];
            
            var svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='1920' height='1080'>
                <rect width='100%' height='100%' fill='{color}'/>
                <text x='50%' y='50%' font-family='Arial' font-size='36' fill='white' 
                      text-anchor='middle' dy='.3em'>Background: {mediaGuid.ToString().Substring(0, 8)}</text>
            </svg>";
            
            return "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        }
    }
}