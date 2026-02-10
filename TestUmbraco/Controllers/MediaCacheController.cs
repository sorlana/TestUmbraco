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
        private readonly IStaticCssGeneratorService _staticCssGenerator;
        private readonly ILoggingService _loggingService;

        public MediaCacheController(
            IMediaCacheService mediaCacheService,
            IStaticCssGeneratorService staticCssGenerator,
            ILoggingService loggingService)
        {
            _mediaCacheService = mediaCacheService;
            _staticCssGenerator = staticCssGenerator;
            _loggingService = loggingService;
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
                _loggingService.LogError<MediaCacheController>($"Error getting cache info", ex);
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
                _loggingService.LogError<MediaCacheController>($"Error clearing cache for media {mediaKey}", ex);
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
                _loggingService.LogError<MediaCacheController>("Error clearing all media cache", ex);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("regenerate-css")]
        public async Task<IActionResult> RegenerateCss()
        {
            try
            {
                await _staticCssGenerator.GenerateBackgroundCssFileAsync();
                
                return Ok(new 
                { 
                    success = true, 
                    message = "CSS file regenerated successfully"
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError<MediaCacheController>("Error regenerating CSS", ex);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}