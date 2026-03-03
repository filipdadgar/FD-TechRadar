namespace TechRadar.Core.Domain;

public class RingChangeHistory
{
    public Guid Id { get; set; }
    public Guid TechnologyEntryId { get; set; }
    public Ring? PreviousRing { get; set; }
    public Ring NewRing { get; set; }
    public Quadrant? PreviousQuadrant { get; set; }
    public Quadrant NewQuadrant { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }

    public TechnologyEntry Entry { get; set; } = null!;
}
