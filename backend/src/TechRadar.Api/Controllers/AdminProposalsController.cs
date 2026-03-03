using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;
using TechRadar.Core.Services;

namespace TechRadar.Api.Controllers;

[ApiController]
[Route("admin/proposals")]
[Authorize]
public class AdminProposalsController(
    IProposalRepository proposalRepo,
    ProposalService proposalService,
    IConfiguration config) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] bool staleOnly = false,
        CancellationToken ct = default)
    {
        ProposalStatus? filterStatus = Enum.TryParse<ProposalStatus>(status, out var ps) ? ps : null;
        int threshold = int.TryParse(config["AGENTS_STALE_PROPOSAL_THRESHOLD_DAYS"], out var t) ? t : 7;
        var proposals = await proposalRepo.GetAllAsync(filterStatus, staleOnly, threshold, ct);
        return Ok(proposals.Select(p => MapProposal(p, threshold)));
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptRequest? req, CancellationToken ct)
    {
        AcceptOverrides? overrides = req == null ? null : new AcceptOverrides(
            req.Name,
            req.Quadrant.HasValue ? req.Quadrant : null,
            req.Ring.HasValue ? req.Ring : null,
            req.Rationale,
            req.Description);

        // If not LLM enriched, quadrant/ring/rationale are required
        var proposal = await proposalRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Proposal {id} not found.");

        if (!proposal.IsLlmEnriched)
        {
            if (overrides?.Quadrant == null || overrides?.Ring == null || string.IsNullOrWhiteSpace(overrides?.Rationale))
                return BadRequest(new { error = "Quadrant, Ring, and Rationale are required when proposal is not LLM-enriched." });
        }

        var reviewer = User.Identity?.Name ?? "admin";
        var result = await proposalService.AcceptAsync(id, overrides, reviewer, ct);
        return Ok(MapProposal(result, 7));
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest? req, CancellationToken ct)
    {
        var reviewer = User.Identity?.Name ?? "admin";
        var result = await proposalService.RejectAsync(id, req?.Reason, reviewer, ct);
        return Ok(MapProposal(result, 7));
    }

    private static object MapProposal(AgentProposal p, int thresholdDays) => new
    {
        id = p.Id,
        proposedName = p.ProposedName,
        recommendedQuadrant = p.RecommendedQuadrant?.ToString(),
        recommendedRing = p.RecommendedRing?.ToString(),
        evidenceSummary = p.EvidenceSummary,
        sourceReferences = JsonSerializer.Deserialize<object>(p.SourceReferences),
        confidenceScore = p.ConfidenceScore,
        isLlmEnriched = p.IsLlmEnriched,
        proposalType = p.ProposalType.ToString(),
        existingEntryId = p.ExistingEntryId,
        status = p.Status.ToString(),
        agentRunId = p.AgentRunId,
        detectedAt = p.DetectedAt,
        isStale = p.Status == ProposalStatus.Pending &&
                  (DateTimeOffset.UtcNow - p.DetectedAt).TotalDays > thresholdDays,
        reviewedAt = p.ReviewedAt,
        reviewedBy = p.ReviewedBy,
        reviewerNotes = p.ReviewerNotes,
        resultingEntryId = p.ResultingEntryId
    };

    public record AcceptRequest(
        string? Name,
        Quadrant? Quadrant,
        Ring? Ring,
        string? Rationale,
        string? Description);

    public record RejectRequest(string? Reason);
}
