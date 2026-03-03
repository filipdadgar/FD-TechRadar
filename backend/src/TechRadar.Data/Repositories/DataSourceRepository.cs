using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Data.Repositories;

public class DataSourceRepository(TechRadarDbContext db) : IDataSourceRepository
{
    public async Task<List<DataSource>> GetAllAsync(bool enabledOnly = false, CancellationToken ct = default)
    {
        var query = db.DataSources.AsQueryable();
        if (enabledOnly)
            query = query.Where(s => s.Enabled);
        return await query.OrderBy(s => s.Name).ToListAsync(ct);
    }

    public async Task<DataSource?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.DataSources.FindAsync([id], ct);

    public async Task<DataSource> CreateAsync(DataSource source, CancellationToken ct = default)
    {
        source.Id = Guid.NewGuid();
        source.CreatedAt = DateTimeOffset.UtcNow;
        db.DataSources.Add(source);
        await db.SaveChangesAsync(ct);
        return source;
    }

    public async Task<DataSource> UpdateAsync(DataSource source, CancellationToken ct = default)
    {
        db.DataSources.Update(source);
        await db.SaveChangesAsync(ct);
        return source;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var source = await db.DataSources.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"DataSource {id} not found.");
        db.DataSources.Remove(source);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = db.DataSources.Where(s => s.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }
}
