using Microsoft.AspNetCore.Mvc;

namespace Users.Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    /// <summary>Checks service health.</summary>
    [HttpGet("/health", Name = "CheckServiceHealth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "ok", service = "users" });
}