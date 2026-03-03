using TechRadar.Core.Domain;

namespace TechRadar.Core.Interfaces;

public interface IAgentRunRepository
{
    Task<List<AgentRunLog>> GetAllAsync(int limit = 50, CancellationToken ct = default);
    Task<AgentRunLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AgentRunLog> CreateAsync(AgentRunLog run, CancellationToken ct = default);
    Task<AgentRunLog> UpdateAsync(AgentRunLog run, CancellationToken ct = default);
}
