using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("admin")]
[Authorize]
public class AdminAgentController(
    IAgentRunRepository runRepo,
    Channel<bool> triggerChannel) : ControllerBase
{
    [HttpPost("agents/trigger")]
    public async Task<IActionResult> TriggerScan(CancellationToken ct)
    {
        await triggerChannel.Writer.WriteAsync(true, ct);
        return Accepted(new { message = "Agent scan triggered." });
    }

    [HttpGet("agent-runs")]
    public async Task<IActionResult> GetRuns([FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var runs = await runRepo.GetAllAsync(limit, ct);
        return Ok(new { runs = runs.Select(MapRun) });
    }

    [HttpGet("agent-runs/{id:guid}")]
    public async Task<IActionResult> GetRun(Guid id, CancellationToken ct)
    {
        var run = await runRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"AgentRunLog {id} not found.");
        return Ok(MapRunDetail(run));
    }

    private static object MapRun(AgentRunLog r) => new
    {
        id = r.Id,
        startedAt = r.StartedAt,
        completedAt = r.CompletedAt,
        triggerType = r.TriggerType,
        sourcesScanned = r.SourcesScanned,
        signalsFound = r.SignalsFound,
        proposalsGenerated = r.ProposalsGenerated,
        proposalsDropped = r.ProposalsDropped,
        status = r.Status.ToString(),
        errorCount = JsonSerializer.Deserialize<List<object>>(r.Errors)?.Count ?? 0
    };

    private static object MapRunDetail(AgentRunLog r) => new
    {
        id = r.Id,
        startedAt = r.StartedAt,
        completedAt = r.CompletedAt,
        triggerType = r.TriggerType,
        sourcesScanned = r.SourcesScanned,
        signalsFound = r.SignalsFound,
        proposalsGenerated = r.ProposalsGenerated,
        proposalsDropped = r.ProposalsDropped,
        status = r.Status.ToString(),
        errors = JsonSerializer.Deserialize<object>(r.Errors)
    };
}
