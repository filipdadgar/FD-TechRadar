using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;
using TechRadar.Core.Services;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("radar")]
public class RadarController(
    RadarService radar,
    ISnapshotRepository snapshots,
    SnapshotComparisonService comparison) : ControllerBase
{
    // ── Public endpoints ──────────────────────────────────────────────────────

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var state = await radar.GetCurrentStateAsync(ct);
        return Ok(state.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(MapEntry)));
    }

    [HttpGet("entries")]
    public async Task<IActionResult> GetEntries(
        [FromQuery] string? quadrant,
        [FromQuery] string? ring,
        [FromQuery] string? tag,
        CancellationToken ct)
    {
        Quadrant? q = Enum.TryParse<Quadrant>(quadrant, out var qv) ? qv : null;
        Ring? r = Enum.TryParse<Ring>(ring, out var rv) ? rv : null;
        var entries = await radar.GetEntriesAsync(q, r, tag, ct);
        return Ok(entries.Select(MapEntry));
    }

    [HttpGet("entries/{id:guid}")]
    public async Task<IActionResult> GetEntry(Guid id, CancellationToken ct)
    {
        var entry = await radar.GetEntryDetailAsync(id, ct);
        return Ok(MapEntryDetail(entry));
    }

    // ── Snapshot endpoints ────────────────────────────────────────────────────

    [HttpGet("snapshots")]
    public async Task<IActionResult> GetSnapshots(CancellationToken ct)
    {
        var all = await snapshots.GetAllAsync(ct);
        return Ok(all.Select(s => new
        {
            id = s.Id,
            capturedAt = s.CapturedAt,
            triggerEvent = s.TriggerEvent,
            triggerEntityId = s.TriggerEntityId,
            entryCount = JsonSerializer.Deserialize<List<object>>(s.Entries)?.Count ?? 0
        }));
    }

    [HttpGet("snapshots/{id:guid}")]
    public async Task<IActionResult> GetSnapshot(Guid id, CancellationToken ct)
    {
        var snapshot = await snapshots.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Snapshot {id} not found.");

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var entries = JsonSerializer.Deserialize<List<SnapshotEntryJson>>(snapshot.Entries, opts) ?? [];

        var grouped = entries.GroupBy(e => e.Quadrant)
            .ToDictionary(g => g.Key, g => g.ToList());

        return Ok(new
        {
            id = snapshot.Id,
            capturedAt = snapshot.CapturedAt,
            triggerEvent = snapshot.TriggerEvent,
            entries = grouped
        });
    }

    [HttpGet("snapshots/compare")]
    public async Task<IActionResult> CompareSnapshots(
        [FromQuery] Guid from, [FromQuery] Guid to, CancellationToken ct)
    {
        if (from == to)
            return BadRequest(new { error = "from and to snapshot IDs must differ." });

        var diff = await comparison.CompareAsync(from, to, ct);
        return Ok(diff);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static object MapEntry(Core.Domain.TechnologyEntry e) => new
    {
        id = e.Id,
        name = e.Name,
        quadrant = e.Quadrant.ToString(),
        ring = e.Ring.ToString(),
        description = e.Description,
        rationale = e.Rationale,
        tags = e.Tags,
        status = e.Status.ToString(),
        lastReviewedAt = e.LastReviewedAt
    };

    private static object MapEntryDetail(Core.Domain.TechnologyEntry e) => new
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
        ringHistory = e.RingHistory.Select(h => new
        {
            changedAt = h.ChangedAt,
            changedBy = h.ChangedBy,
            previousRing = h.PreviousRing?.ToString(),
            newRing = h.NewRing.ToString(),
            previousQuadrant = h.PreviousQuadrant?.ToString(),
            newQuadrant = h.NewQuadrant.ToString(),
            changeReason = h.ChangeReason
        })
    };

    private record SnapshotEntryJson(Guid Id, string Name, string Quadrant, string Ring, string Description, string Rationale);
}
