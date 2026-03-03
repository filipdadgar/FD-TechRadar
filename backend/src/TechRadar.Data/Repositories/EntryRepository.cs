using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Data.Repositories;

public class EntryRepository(TechRadarDbContext db) : IEntryRepository
{
    public async Task<List<TechnologyEntry>> GetActiveAsync(
        Quadrant? quadrant = null, Ring? ring = null, string? tag = null,
        CancellationToken ct = default)
    {
        var query = db.TechnologyEntries
            .Where(e => e.Status == EntryStatus.Active);

        if (quadrant.HasValue)
            query = query.Where(e => e.Quadrant == quadrant.Value);

        if (ring.HasValue)
            query = query.Where(e => e.Ring == ring.Value);

        if (!string.IsNullOrWhiteSpace(tag))
            query = query.Where(e => e.Tags.Contains(tag));

        return await query.OrderBy(e => e.Name).ToListAsync(ct);
    }

    public async Task<TechnologyEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.TechnologyEntries
            .Include(e => e.RingHistory.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<List<TechnologyEntry>> GetAllAsync(bool includeArchived = false, CancellationToken ct = default)
    {
        var query = db.TechnologyEntries.AsQueryable();
        if (!includeArchived)
            query = query.Where(e => e.Status == EntryStatus.Active);
        return await query.OrderBy(e => e.Name).ToListAsync(ct);
    }

    public async Task<List<RingChangeHistory>> GetRingHistoryAsync(Guid entryId, CancellationToken ct = default)
        => await db.RingChangeHistories
            .Where(h => h.TechnologyEntryId == entryId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(ct);

    public async Task<TechnologyEntry> CreateAsync(TechnologyEntry entry, CancellationToken ct = default)
    {
        entry.Id = Guid.NewGuid();
        entry.CreatedAt = DateTimeOffset.UtcNow;
        entry.LastReviewedAt = DateTimeOffset.UtcNow;
        db.TechnologyEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<TechnologyEntry> UpdateAsync(TechnologyEntry entry, CancellationToken ct = default)
    {
        entry.LastReviewedAt = DateTimeOffset.UtcNow;
        db.TechnologyEntries.Update(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await db.TechnologyEntries.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"TechnologyEntry {id} not found.");
        entry.Status = EntryStatus.Archived;
        entry.LastReviewedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = db.TechnologyEntries.Where(e => e.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }
}
