namespace TechRadar.Core.Interfaces;

using TechRadar.Core.Domain;

public record ClassificationResult(
    Quadrant? RecommendedQuadrant,
    Ring? RecommendedRing,
    string? EvidenceSummary,
    float? ConfidenceScore,
    bool IsEnriched
);

public interface ILlmClassifier
{
    Task<ClassificationResult> ClassifyAsync(
        string name,
        string rawDescription,
        IEnumerable<string> sourceUrls,
        CancellationToken ct = default);
}
