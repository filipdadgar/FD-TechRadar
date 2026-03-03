using TechRadar.Core.Domain;

namespace TechRadar.Core.Interfaces;

public interface IDataSourceRepository
{
    Task<List<DataSource>> GetAllAsync(bool enabledOnly = false, CancellationToken ct = default);
    Task<DataSource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DataSource> CreateAsync(DataSource source, CancellationToken ct = default);
    Task<DataSource> UpdateAsync(DataSource source, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
