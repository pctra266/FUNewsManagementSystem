using Microsoft.AspNetCore.Mvc;

namespace Presentation_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint for connectivity testing
        /// Returns 200 OK if API is reachable
        /// </summary>
        [HttpGet]
        [HttpHead]
        public IActionResult Check()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
