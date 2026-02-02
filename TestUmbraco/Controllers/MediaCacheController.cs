using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestUmbraco.Services;
using Umbraco.Cms.Web.Common.Controllers;

namespace TestUmbraco.Controllers
{
    [Route("umbraco/backoffice/api/media-cache")]
    [ApiController]
    [Authorize(Policy = "BackOffice")]
    public class MediaCacheController : ControllerBase
    {
        private readonly IMediaCacheService _mediaCacheService;
        private readonly ILogger<MediaCacheController> _logger;

        public MediaCacheController(
            IMediaCacheService mediaCacheService,
            ILogger<MediaCacheController> logger)
        {
            _mediaCacheService = mediaCacheService;
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
                _mediaCacheService.ClearCacheForMedia(mediaKey);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Cache cleared for media {mediaKey}"
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
                _mediaCacheService.ClearAllCache();
                
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