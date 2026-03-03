namespace TechRadar.Core.Domain;

public class AgentProposal
{
    public Guid Id { get; set; }
    public string ProposedName { get; set; } = string.Empty;
    public Quadrant? RecommendedQuadrant { get; set; }
    public Ring? RecommendedRing { get; set; }
    public string? EvidenceSummary { get; set; }
    /// <summary>JSON array of {title, url, publishedAt}.</summary>
    public string SourceReferences { get; set; } = "[]";
    public float? ConfidenceScore { get; set; }
    public bool IsLlmEnriched { get; set; } = false;
    public ProposalType ProposalType { get; set; }
    public Guid? ExistingEntryId { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public Guid AgentRunId { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewerNotes { get; set; }
    public Guid? ResultingEntryId { get; set; }

    public AgentRunLog AgentRun { get; set; } = null!;
    public TechnologyEntry? ExistingEntry { get; set; }
    public TechnologyEntry? ResultingEntry { get; set; }
}
