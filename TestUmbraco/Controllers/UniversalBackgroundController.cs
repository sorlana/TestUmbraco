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

        public UniversalBackgroundController(
            IMediaCacheService mediaCacheService,
            ILogger<UniversalBackgroundController> logger)
        {
            _mediaCacheService = mediaCacheService;
            _logger = logger;
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
    }
}