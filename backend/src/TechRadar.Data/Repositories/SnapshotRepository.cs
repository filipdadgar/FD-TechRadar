using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Data.Repositories;

public class SnapshotRepository(TechRadarDbContext db) : ISnapshotRepository
{
    public async Task<List<RadarSnapshot>> GetAllAsync(CancellationToken ct = default)
        => await db.RadarSnapshots
            .OrderByDescending(s => s.CapturedAt)
            .ToListAsync(ct);

    public async Task<RadarSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.RadarSnapshots.FindAsync([id], ct);

    public async Task<RadarSnapshot> CreateAsync(RadarSnapshot snapshot, CancellationToken ct = default)
    {
        snapshot.Id = Guid.NewGuid();
        snapshot.CapturedAt = DateTimeOffset.UtcNow;
        db.RadarSnapshots.Add(snapshot);
        await db.SaveChangesAsync(ct);
        return snapshot;
    }
}
