using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("admin/sources")]
[Authorize]
public class AdminSourcesController(IDataSourceRepository sourceRepo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sources = await sourceRepo.GetAllAsync(ct: ct);
        return Ok(sources.Select(MapSource));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SourceRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<SourceType>(req.SourceType, out var sourceType))
            return BadRequest(new { error = $"Invalid sourceType: {req.SourceType}" });

        if (await sourceRepo.ExistsByNameAsync(req.Name, ct: ct))
            throw new InvalidOperationException($"conflict: A source named '{req.Name}' already exists.");

        var source = new DataSource
        {
            Name = req.Name,
            SourceType = sourceType,
            ConnectionDetails = req.ConnectionDetails,
            Enabled = req.Enabled ?? true
        };

        var created = await sourceRepo.CreateAsync(source, ct);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, MapSource(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SourceRequest req, CancellationToken ct)
    {
        var existing = await sourceRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"DataSource {id} not found.");

        if (!Enum.TryParse<SourceType>(req.SourceType, out var sourceType))
            return BadRequest(new { error = $"Invalid sourceType: {req.SourceType}" });

        if (req.Name != existing.Name && await sourceRepo.ExistsByNameAsync(req.Name, id, ct))
            throw new InvalidOperationException($"conflict: A source named '{req.Name}' already exists.");

        existing.Name = req.Name;
        existing.SourceType = sourceType;
        existing.ConnectionDetails = req.ConnectionDetails;
        existing.Enabled = req.Enabled ?? existing.Enabled;

        var updated = await sourceRepo.UpdateAsync(existing, ct);
        return Ok(MapSource(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sourceRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    private static object MapSource(DataSource s) => new
    {
        id = s.Id,
        name = s.Name,
        sourceType = s.SourceType.ToString(),
        connectionDetails = JsonSerializer.Deserialize<object>(s.ConnectionDetails),
        enabled = s.Enabled,
        createdAt = s.CreatedAt,
        lastSuccessfulScanAt = s.LastSuccessfulScanAt
    };

    public record SourceRequest(
        string Name,
        string SourceType,
        string ConnectionDetails,
        bool? Enabled);
}
