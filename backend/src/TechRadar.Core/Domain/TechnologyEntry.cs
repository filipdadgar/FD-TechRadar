namespace TechRadar.Core.Domain;

public class TechnologyEntry
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Quadrant Quadrant { get; set; }
    public Ring Ring { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public EntryStatus Status { get; set; } = EntryStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastReviewedAt { get; set; }
    public Guid? CreatedFromProposalId { get; set; }

    public ICollection<RingChangeHistory> RingHistory { get; set; } = [];
}
