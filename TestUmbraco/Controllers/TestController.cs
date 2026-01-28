// Controllers/TestController.cs
using Microsoft.AspNetCore.Mvc;

namespace TestUmbraco.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("css")]
        public IActionResult GetTestCss()
        {
            var css = @"
/* Test CSS from API */
.test-class {
    background-color: #f0f0f0;
    color: #333;
    padding: 20px;
    border: 1px solid #ccc;
}";
            
            return Content(css, "text/css");
        }
    }
}