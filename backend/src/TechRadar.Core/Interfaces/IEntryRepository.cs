using TechRadar.Core.Domain;

namespace TechRadar.Core.Interfaces;

public interface IEntryRepository
{
    Task<List<TechnologyEntry>> GetActiveAsync(Quadrant? quadrant = null, Ring? ring = null, string? tag = null, CancellationToken ct = default);
    Task<TechnologyEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TechnologyEntry>> GetAllAsync(bool includeArchived = false, CancellationToken ct = default);
    Task<List<RingChangeHistory>> GetRingHistoryAsync(Guid entryId, CancellationToken ct = default);
    Task<TechnologyEntry> CreateAsync(TechnologyEntry entry, CancellationToken ct = default);
    Task<TechnologyEntry> UpdateAsync(TechnologyEntry entry, CancellationToken ct = default);
    Task ArchiveAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
