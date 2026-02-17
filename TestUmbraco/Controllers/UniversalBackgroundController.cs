using Microsoft.AspNetCore.Mvc;
using TestUmbraco.Services;

namespace TestUmbraco.Controllers
{
    [Route("api/background")]
    [ApiController]
    public class UniversalBackgroundController : ControllerBase
    {
        private readonly IMediaCacheService _mediaCacheService;
        private readonly ILogger<UniversalBackgroundController> _logger;
        private readonly IStaticCssGeneratorService _cssGeneratorService;

        public UniversalBackgroundController(
            IMediaCacheService mediaCacheService,
            ILogger<UniversalBackgroundController> logger,
            IStaticCssGeneratorService cssGeneratorService)
        {
            _mediaCacheService = mediaCacheService;
            _logger = logger;
            _cssGeneratorService = cssGeneratorService;
        }

        [HttpGet("multiple")]
        public async Task<IActionResult> GetMultipleCss([FromQuery] string backgrounds)
        {
            if (string.IsNullOrEmpty(backgrounds))
                return Content("/* No backgrounds specified */", "text/css");

            var css = await _mediaCacheService.GetCachedBackgroundCssAsync(backgrounds);
            
            if (string.IsNullOrWhiteSpace(css))
                css = "/* No valid backgrounds generated */";
            
            return Content(css, "text/css");
        }
        
        [HttpPost("regenerate-all")]
        public async Task<IActionResult> RegenerateAllCss()
        {
            try
            {
                await _cssGeneratorService.RegenerateAllCssAsync();
                return Ok("CSS regenerated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating CSS");
                return StatusCode(500, "Error regenerating CSS: " + ex.Message);
            }
        }
    }
}