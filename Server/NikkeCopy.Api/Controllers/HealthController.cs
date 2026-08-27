using Microsoft.AspNetCore.Mvc;

namespace NikkeCopy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("ok", DateTimeOffset.UtcNow));
    }
}

public sealed record HealthResponse(string Status, DateTimeOffset ServerTime);
