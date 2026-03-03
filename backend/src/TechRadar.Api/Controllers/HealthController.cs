using Microsoft.AspNetCore.Mvc;
using TechRadar.Data;

namespace TechRadar.Api.Controllers;

[ApiController]
public class HealthController(TechRadarDbContext db) : ControllerBase
{
    [HttpGet("/healthz")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        try
        {
            var connected = await db.Database.CanConnectAsync(ct);
            if (connected)
                return Ok(new { status = "healthy", db = "connected" });
            return StatusCode(503, new { status = "unhealthy", db = "unreachable" });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy", db = "unreachable" });
        }
    }
}
