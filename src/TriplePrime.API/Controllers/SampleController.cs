using Microsoft.AspNetCore.Mvc;

namespace TriplePrime.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is running!" });
        }
    }
} 