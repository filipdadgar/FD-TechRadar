using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

public class EntryService(IEntryRepository entries, SnapshotService snapshots)
{
    public async Task<TechnologyEntry> CreateAsync(
        TechnologyEntry entry, Guid? fromProposalId = null, CancellationToken ct = default)
    {
        if (await entries.ExistsByNameAsync(entry.Name, ct: ct))
            throw new InvalidOperationException($"conflict: A technology named '{entry.Name}' already exists.");

        entry.CreatedFromProposalId = fromProposalId;

        var created = await entries.CreateAsync(entry, ct);

        created.RingHistory.Add(new RingChangeHistory
        {
            Id = Guid.NewGuid(),
            TechnologyEntryId = created.Id,
            PreviousRing = null,
            NewRing = created.Ring,
            PreviousQuadrant = null,
            NewQuadrant = created.Quadrant,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = fromProposalId.HasValue ? "agent:accepted" : "admin"
        });

        await entries.UpdateAsync(created, ct);
        await snapshots.CaptureAsync("manual-edit", created.Id, ct);
        return created;
    }

    public async Task<TechnologyEntry> UpdateAsync(
        Guid id, TechnologyEntry updates, string changedBy = "admin",
        string? changeReason = null, CancellationToken ct = default)
    {
        var existing = await entries.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"TechnologyEntry {id} not found.");

        if (updates.Name != existing.Name && await entries.ExistsByNameAsync(updates.Name, id, ct))
            throw new InvalidOperationException($"conflict: A technology named '{updates.Name}' already exists.");

        var ringChanged   = updates.Ring != existing.Ring;
        var quadrantChanged = updates.Quadrant != existing.Quadrant;

        existing.Name        = updates.Name;
        existing.Description = updates.Description;
        existing.Rationale   = updates.Rationale;
        existing.Tags        = updates.Tags;

        if (ringChanged || quadrantChanged)
        {
            var prev = existing.Ring;
            var prevQ = existing.Quadrant;
            existing.Ring     = updates.Ring;
            existing.Quadrant = updates.Quadrant;
            existing.RingHistory.Add(new RingChangeHistory
            {
                Id = Guid.NewGuid(),
                TechnologyEntryId = existing.Id,
                PreviousRing = prev,
                NewRing = updates.Ring,
                PreviousQuadrant = quadrantChanged ? prevQ : null,
                NewQuadrant = updates.Quadrant,
                ChangedAt = DateTimeOffset.UtcNow,
                ChangedBy = changedBy,
                ChangeReason = changeReason
            });
        }

        var updated = await entries.UpdateAsync(existing, ct);
        await snapshots.CaptureAsync("manual-edit", updated.Id, ct);
        return updated;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await entries.ArchiveAsync(id, ct);
        await snapshots.CaptureAsync("manual-edit", id, ct);
    }
}
