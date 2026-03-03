using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Data.Repositories;

public class AgentRunRepository(TechRadarDbContext db) : IAgentRunRepository
{
    public async Task<List<AgentRunLog>> GetAllAsync(int limit = 50, CancellationToken ct = default)
        => await db.AgentRunLogs
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<AgentRunLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.AgentRunLogs.FindAsync([id], ct);

    public async Task<AgentRunLog> CreateAsync(AgentRunLog run, CancellationToken ct = default)
    {
        run.Id = Guid.NewGuid();
        run.StartedAt = DateTimeOffset.UtcNow;
        db.AgentRunLogs.Add(run);
        await db.SaveChangesAsync(ct);
        return run;
    }

    public async Task<AgentRunLog> UpdateAsync(AgentRunLog run, CancellationToken ct = default)
    {
        db.AgentRunLogs.Update(run);
        await db.SaveChangesAsync(ct);
        return run;
    }
}
