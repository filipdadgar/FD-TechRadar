using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

public class RadarService(IEntryRepository entries)
{
    public async Task<Dictionary<string, List<TechnologyEntry>>> GetCurrentStateAsync(
        CancellationToken ct = default)
    {
        var all = await entries.GetActiveAsync(ct: ct);
        return all.GroupBy(e => e.Quadrant.ToString())
                  .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Name).ToList());
    }

    public async Task<List<TechnologyEntry>> GetEntriesAsync(
        Quadrant? quadrant, Ring? ring, string? tag, CancellationToken ct = default)
        => await entries.GetActiveAsync(quadrant, ring, tag, ct);

    public async Task<TechnologyEntry> GetEntryDetailAsync(Guid id, CancellationToken ct = default)
        => await entries.GetByIdAsync(id, ct)
           ?? throw new KeyNotFoundException($"Entry {id} not found.");
}
