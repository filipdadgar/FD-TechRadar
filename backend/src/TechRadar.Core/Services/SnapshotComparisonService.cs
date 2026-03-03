using System.Text.Json;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

public record SnapshotEntry(Guid Id, string Name, string Quadrant, string Ring, string Description, string Rationale);

public record MovedEntry(string Name, string FromQuadrant, string ToQuadrant, string FromRing, string ToRing);

public record SnapshotDiff(
    Guid FromSnapshotId, DateTimeOffset FromCapturedAt,
    Guid ToSnapshotId, DateTimeOffset ToCapturedAt,
    List<SnapshotEntry> Added,
    List<SnapshotEntry> Removed,
    List<MovedEntry> Moved,
    int UnchangedCount);

public class SnapshotComparisonService(ISnapshotRepository snapshots)
{
    public async Task<SnapshotDiff> CompareAsync(Guid fromId, Guid toId, CancellationToken ct = default)
    {
        var from = await snapshots.GetByIdAsync(fromId, ct)
            ?? throw new KeyNotFoundException($"Snapshot {fromId} not found.");
        var to = await snapshots.GetByIdAsync(toId, ct)
            ?? throw new KeyNotFoundException($"Snapshot {toId} not found.");

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fromEntries = JsonSerializer.Deserialize<List<SnapshotEntry>>(from.Entries, opts) ?? [];
        var toEntries = JsonSerializer.Deserialize<List<SnapshotEntry>>(to.Entries, opts) ?? [];

        var fromByName = fromEntries.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);
        var toByName = toEntries.ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);

        var added = toEntries.Where(e => !fromByName.ContainsKey(e.Name)).ToList();
        var removed = fromEntries.Where(e => !toByName.ContainsKey(e.Name)).ToList();
        var moved = fromEntries
            .Where(e => toByName.ContainsKey(e.Name))
            .Select(e => (From: e, To: toByName[e.Name]))
            .Where(p => p.From.Ring != p.To.Ring || p.From.Quadrant != p.To.Quadrant)
            .Select(p => new MovedEntry(p.From.Name, p.From.Quadrant, p.To.Quadrant, p.From.Ring, p.To.Ring))
            .ToList();
        var unchanged = fromEntries
            .Count(e => toByName.ContainsKey(e.Name) &&
                        e.Ring == toByName[e.Name].Ring &&
                        e.Quadrant == toByName[e.Name].Quadrant);

        return new SnapshotDiff(from.Id, from.CapturedAt, to.Id, to.CapturedAt,
            added, removed, moved, unchanged);
    }
}
