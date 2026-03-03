using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;
using TechRadar.Core.Services;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("admin/entries")]
[Authorize]
public class AdminEntriesController(
    EntryService entryService,
    IEntryRepository entryRepo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
    {
        bool includeArchived = status?.ToLower() == "all";
        var entries = await entryRepo.GetAllAsync(includeArchived, ct);
        return Ok(entries.Select(MapEntry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EntryRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<Quadrant>(req.Quadrant, out var quadrant))
            return BadRequest(new { error = $"Invalid quadrant: {req.Quadrant}" });
        if (!Enum.TryParse<Ring>(req.Ring, out var ring))
            return BadRequest(new { error = $"Invalid ring: {req.Ring}" });

        var entry = new TechnologyEntry
        {
            Name = req.Name,
            Description = req.Description,
            Rationale = req.Rationale,
            Quadrant = quadrant,
            Ring = ring,
            Tags = req.Tags ?? []
        };

        var created = await entryService.CreateAsync(entry);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, MapEntry(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EntryRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<Quadrant>(req.Quadrant, out var quadrant))
            return BadRequest(new { error = $"Invalid quadrant: {req.Quadrant}" });
        if (!Enum.TryParse<Ring>(req.Ring, out var ring))
            return BadRequest(new { error = $"Invalid ring: {req.Ring}" });

        var updates = new TechnologyEntry
        {
            Name = req.Name,
            Description = req.Description,
            Rationale = req.Rationale,
            Quadrant = quadrant,
            Ring = ring,
            Tags = req.Tags ?? []
        };

        var updated = await entryService.UpdateAsync(id, updates, "admin", req.ChangeReason, ct);
        return Ok(MapEntry(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        await entryService.ArchiveAsync(id, ct);
        return NoContent();
    }

    private static object MapEntry(TechnologyEntry e) => new
    {
        id = e.Id,
        name = e.Name,
        quadrant = e.Quadrant.ToString(),
        ring = e.Ring.ToString(),
        description = e.Description,
        rationale = e.Rationale,
        tags = e.Tags,
        status = e.Status.ToString(),
        createdAt = e.CreatedAt,
        lastReviewedAt = e.LastReviewedAt,
        createdFromProposalId = e.CreatedFromProposalId
    };

    public record EntryRequest(
        string Name,
        string Description,
        string Rationale,
        string Quadrant,
        string Ring,
        List<string>? Tags,
        string? ChangeReason);
}
