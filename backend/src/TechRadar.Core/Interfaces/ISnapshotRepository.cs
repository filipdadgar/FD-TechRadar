using TechRadar.Core.Domain;

namespace TechRadar.Core.Interfaces;

public interface ISnapshotRepository
{
    Task<List<RadarSnapshot>> GetAllAsync(CancellationToken ct = default);
    Task<RadarSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RadarSnapshot> CreateAsync(RadarSnapshot snapshot, CancellationToken ct = default);
}
