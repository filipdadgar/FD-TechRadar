namespace TechRadar.Core.Domain;

public class AgentRunLog
{
    public Guid Id { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public int SourcesScanned { get; set; }
    public int SignalsFound { get; set; }
    public int ProposalsGenerated { get; set; }
    public int ProposalsDropped { get; set; }
    /// <summary>JSON array of {sourceId, message, occurredAt}.</summary>
    public string Errors { get; set; } = "[]";
    public RunStatus Status { get; set; }

    public ICollection<AgentProposal> Proposals { get; set; } = [];
}
