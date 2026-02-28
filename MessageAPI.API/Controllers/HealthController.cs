using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    public class HealthController : BaseController
    {
        [HttpGet]
        public IActionResult Health()
            => Ok(new { status = "healthy", timestamp = DateTime.UtcNow, version = "1.0.0" });
    }
}
