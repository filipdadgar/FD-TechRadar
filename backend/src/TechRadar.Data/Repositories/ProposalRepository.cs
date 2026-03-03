using Microsoft.EntityFrameworkCore;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Data.Repositories;

public class ProposalRepository(TechRadarDbContext db) : IProposalRepository
{
    public async Task<List<AgentProposal>> GetAllAsync(
        ProposalStatus? status = null, bool staleOnly = false, int thresholdDays = 7,
        CancellationToken ct = default)
    {
        var query = db.AgentProposals.AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (staleOnly)
        {
            var cutoff = DateTimeOffset.UtcNow.AddDays(-thresholdDays);
            query = query.Where(p => p.Status == ProposalStatus.Pending && p.DetectedAt < cutoff);
        }

        return await query.OrderByDescending(p => p.DetectedAt).ToListAsync(ct);
    }

    public async Task<AgentProposal?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.AgentProposals.FindAsync([id], ct);

    public async Task<AgentProposal> CreateAsync(AgentProposal proposal, CancellationToken ct = default)
    {
        proposal.Id = Guid.NewGuid();
        proposal.DetectedAt = DateTimeOffset.UtcNow;
        db.AgentProposals.Add(proposal);
        await db.SaveChangesAsync(ct);
        return proposal;
    }

    public async Task<AgentProposal> UpdateAsync(AgentProposal proposal, CancellationToken ct = default)
    {
        db.AgentProposals.Update(proposal);
        await db.SaveChangesAsync(ct);
        return proposal;
    }

    public async Task<AgentProposal?> FindPendingByNameAsync(string name, CancellationToken ct = default)
        => await db.AgentProposals
            .Where(p => p.Status == ProposalStatus.Pending &&
                        p.ProposedName.ToLower() == name.ToLower())
            .FirstOrDefaultAsync(ct);
}
