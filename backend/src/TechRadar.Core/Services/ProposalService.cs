using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

public record AcceptOverrides(
    string? Name,
    Quadrant? Quadrant,
    Ring? Ring,
    string? Rationale,
    string? Description);

public class ProposalService(
    IProposalRepository proposals,
    EntryService entryService,
    IEntryRepository entries)
{
    public async Task<AgentProposal> AcceptAsync(
        Guid id, AcceptOverrides? overrides = null, string reviewer = "admin",
        CancellationToken ct = default)
    {
        var proposal = await proposals.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Proposal {id} not found.");

        if (proposal.Status != ProposalStatus.Pending)
            throw new InvalidOperationException($"conflict: Proposal {id} has already been actioned.");

        var name = overrides?.Name ?? proposal.ProposedName;
        var quadrant = overrides?.Quadrant ?? proposal.RecommendedQuadrant
            ?? throw new ArgumentException("Quadrant must be specified.");
        var ring = overrides?.Ring ?? proposal.RecommendedRing
            ?? throw new ArgumentException("Ring must be specified.");
        var rationale = overrides?.Rationale ?? proposal.EvidenceSummary
            ?? throw new ArgumentException("Rationale must be specified.");
        var description = overrides?.Description ?? proposal.ProposedName;

        bool edited = overrides != null && (
            overrides.Name != null ||
            overrides.Quadrant != null ||
            overrides.Ring != null ||
            overrides.Rationale != null ||
            overrides.Description != null);

        TechnologyEntry resultingEntry;

        if (proposal.ProposalType == ProposalType.RingChange && proposal.ExistingEntryId.HasValue)
        {
            var existing = await entries.GetByIdAsync(proposal.ExistingEntryId.Value, ct)
                ?? throw new KeyNotFoundException($"Existing entry {proposal.ExistingEntryId} not found.");

            existing.Ring = ring;
            existing.Quadrant = quadrant;
            resultingEntry = await entryService.UpdateAsync(
                existing.Id, existing, "agent:accepted", $"Accepted from proposal {id}", ct);
        }
        else
        {
            var entry = new TechnologyEntry
            {
                Name = name,
                Quadrant = quadrant,
                Ring = ring,
                Rationale = rationale,
                Description = description,
                Tags = []
            };
            resultingEntry = await entryService.CreateAsync(entry, id, ct);
        }

        proposal.Status = edited ? ProposalStatus.EditedAndAccepted : ProposalStatus.Accepted;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        proposal.ReviewedBy = reviewer;
        proposal.ResultingEntryId = resultingEntry.Id;

        return await proposals.UpdateAsync(proposal, ct);
    }

    public async Task<AgentProposal> RejectAsync(
        Guid id, string? reason = null, string reviewer = "admin",
        CancellationToken ct = default)
    {
        var proposal = await proposals.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Proposal {id} not found.");

        if (proposal.Status != ProposalStatus.Pending)
            throw new InvalidOperationException($"conflict: Proposal {id} has already been actioned.");

        proposal.Status = ProposalStatus.Rejected;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        proposal.ReviewedBy = reviewer;
        proposal.ReviewerNotes = reason;

        return await proposals.UpdateAsync(proposal, ct);
    }
}
