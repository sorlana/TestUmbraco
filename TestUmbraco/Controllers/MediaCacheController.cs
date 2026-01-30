//MediaCacheController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Web.Common.Controllers;

namespace TestUmbraco.Controllers
{
    [Route("umbraco/backoffice/api/media-cache")]
    [ApiController]
    [Authorize(Policy = "BackOffice")]
    public class MediaCacheController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly IAppPolicyCache _runtimeCache;
        private readonly ILogger<MediaCacheController> _logger;

        public MediaCacheController(
            IMediaService mediaService,
            AppCaches appCaches,
            ILogger<MediaCacheController> logger)
        {
            _mediaService = mediaService;
            _runtimeCache = appCaches.RuntimeCache;
            _logger = logger;
        }

        [HttpGet("info")]
        public IActionResult GetCacheInfo()
        {
            try
            {
                var cacheStats = new
                {
                    MemoryUsage = GC.GetTotalMemory(false) / 1024 / 1024 + "MB",
                    Timestamp = DateTime.UtcNow
                };
                
                return Ok(cacheStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache info");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("clear-for-media")]
        public IActionResult ClearCacheForMedia([FromBody] Guid mediaKey)
        {
            try
            {
                var cacheKey = $"media_{mediaKey}";
                _runtimeCache.Clear(cacheKey);
                
                _logger.LogInformation($"Cache cleared for media: {mediaKey}");
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Cache cleared for media {mediaKey}",
                    cacheKey = cacheKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing cache for media {mediaKey}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("clear-all")]
        public IActionResult ClearAllMediaCache()
        {
            try
            {
                _runtimeCache.Clear();
                
                _logger.LogInformation("Cleared all cache entries");
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Cleared all cache entries"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all media cache");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}