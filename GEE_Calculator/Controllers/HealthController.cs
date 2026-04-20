using Microsoft.AspNetCore.Mvc;

namespace GEE_Calculator.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service = "GEE_Calculator",
            status = "Healthy",
            utcNow = DateTimeOffset.UtcNow
        });
    }
}
