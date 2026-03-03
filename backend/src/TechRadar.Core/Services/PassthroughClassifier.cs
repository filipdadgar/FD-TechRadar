using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

/// <summary>
/// Used when ANTHROPIC_API_KEY is absent. Returns an unenriched classification
/// so the admin can supply quadrant/ring/rationale during proposal review.
/// </summary>
public class PassthroughClassifier : ILlmClassifier
{
    public Task<ClassificationResult> ClassifyAsync(
        string name, string rawDescription, IEnumerable<string> sourceUrls,
        CancellationToken ct = default)
        => Task.FromResult(new ClassificationResult(
            RecommendedQuadrant: null,
            RecommendedRing: null,
            EvidenceSummary: null,
            ConfidenceScore: null,
            IsEnriched: false));
}
