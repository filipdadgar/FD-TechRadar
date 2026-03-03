namespace TechRadar.Core.Domain;

public class DataSource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SourceType SourceType { get; set; }
    /// <summary>JSON object — type-specific config.</summary>
    public string ConnectionDetails { get; set; } = "{}";
    public bool Enabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastSuccessfulScanAt { get; set; }
}
