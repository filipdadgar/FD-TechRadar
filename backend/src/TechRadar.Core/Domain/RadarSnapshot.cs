namespace TechRadar.Core.Domain;

public class RadarSnapshot
{
    public Guid Id { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public string TriggerEvent { get; set; } = string.Empty;
    public Guid? TriggerEntityId { get; set; }
    /// <summary>JSON array of snapshot entry objects.</summary>
    public string Entries { get; set; } = "[]";
}
