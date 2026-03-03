using System.Text.Json;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

public class SnapshotService(IEntryRepository entries, ISnapshotRepository snapshots)
{
    public async Task<RadarSnapshot> CaptureAsync(
        string triggerEvent, Guid? triggerEntityId = null, CancellationToken ct = default)
    {
        var activeEntries = await entries.GetActiveAsync(ct: ct);

        var snapshotItems = activeEntries.Select(e => new
        {
            id = e.Id,
            name = e.Name,
            quadrant = e.Quadrant.ToString(),
            ring = e.Ring.ToString(),
            description = e.Description,
            rationale = e.Rationale
        });

        var snapshot = new RadarSnapshot
        {
            TriggerEvent = triggerEvent,
            TriggerEntityId = triggerEntityId,
            Entries = JsonSerializer.Serialize(snapshotItems)
        };

        return await snapshots.CreateAsync(snapshot, ct);
    }
}
