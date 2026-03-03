using TechRadar.Core.Domain;

namespace TechRadar.Core.Interfaces;

public interface IProposalRepository
{
    Task<List<AgentProposal>> GetAllAsync(ProposalStatus? status = null, bool staleOnly = false, int thresholdDays = 7, CancellationToken ct = default);
    Task<AgentProposal?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AgentProposal> CreateAsync(AgentProposal proposal, CancellationToken ct = default);
    Task<AgentProposal> UpdateAsync(AgentProposal proposal, CancellationToken ct = default);
    Task<AgentProposal?> FindPendingByNameAsync(string name, CancellationToken ct = default);
}
