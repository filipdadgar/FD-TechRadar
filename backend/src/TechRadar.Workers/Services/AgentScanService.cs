using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;
using TechRadar.Workers.Collectors;

namespace TechRadar.Workers.Services;

public class AgentScanService(
    IAgentRunRepository runRepo,
    IDataSourceRepository sourceRepo,
    IProposalRepository proposalRepo,
    IEntryRepository entryRepo,
    ILlmClassifier classifier,
    RssFeedCollector rssCollector,
    GitHubTopicsCollector githubCollector,
    IConfiguration config,
    ILogger<AgentScanService> logger)
{
    public async Task RunAsync(string triggerType, CancellationToken ct = default)
    {
        var maxProposals = int.TryParse(config["AGENTS_MAX_PROPOSALS_PER_RUN"], out var mp) ? mp : 50;

        var run = await runRepo.CreateAsync(new AgentRunLog
        {
            TriggerType = triggerType,
            Status = RunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        }, ct);

        logger.LogInformation("Agent run {RunId} started (trigger: {Trigger})", run.Id, triggerType);

        var errors = new List<object>();
        var allSignals = new List<FeedSignal>();

        var sources = await sourceRepo.GetAllAsync(enabledOnly: true, ct: ct);
        run.SourcesScanned = sources.Count;

        foreach (var source in sources)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var signals = source.SourceType switch
                {
                    SourceType.RssFeed => await rssCollector.CollectAsync(source, ct),
                    SourceType.GitHubTopics => await githubCollector.CollectAsync(source, ct),
                    _ => []
                };
                allSignals.AddRange(signals);
                logger.LogInformation("Source {SourceName}: {Count} signals", source.Name, signals.Count);

                source.LastSuccessfulScanAt = DateTimeOffset.UtcNow;
                await sourceRepo.UpdateAsync(source, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error scanning source {SourceId} ({SourceName})", source.Id, source.Name);
                errors.Add(new { sourceId = source.Id, message = ex.Message, occurredAt = DateTimeOffset.UtcNow });
            }
        }

        run.SignalsFound = allSignals.Count;

        // Deduplicate: skip signals matching existing Active entries or pending proposals
        var activeEntries = await entryRepo.GetActiveAsync(ct: ct);
        var activeNames = activeEntries.Select(e => e.Name.ToLowerInvariant()).ToHashSet();

        var proposals = new List<AgentProposal>();

        foreach (var signal in allSignals)
        {
            if (proposals.Count >= maxProposals)
            {
                run.ProposalsDropped++;
                continue;
            }

            var name = signal.Title.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            // Skip if already an active entry with the same name
            if (activeNames.Contains(name.ToLowerInvariant())) continue;

            // Skip if already a pending proposal
            var existing = await proposalRepo.FindPendingByNameAsync(name, ct);
            if (existing != null) continue;

            var classification = await classifier.ClassifyAsync(name, signal.Summary, [signal.Link], ct);

            var matchingEntry = activeEntries
                .FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

            var proposalType = matchingEntry != null ? ProposalType.RingChange : ProposalType.NewEntry;

            var source = new { title = signal.Title, url = signal.Link, publishedAt = signal.PublishedAt };

            proposals.Add(new AgentProposal
            {
                ProposedName = name,
                RecommendedQuadrant = classification.RecommendedQuadrant,
                RecommendedRing = classification.RecommendedRing,
                EvidenceSummary = classification.EvidenceSummary,
                ConfidenceScore = classification.ConfidenceScore,
                IsLlmEnriched = classification.IsEnriched,
                SourceReferences = JsonSerializer.Serialize(new[] { source }),
                ProposalType = proposalType,
                ExistingEntryId = matchingEntry?.Id,
                Status = ProposalStatus.Pending,
                AgentRunId = run.Id
            });
        }

        foreach (var proposal in proposals)
            await proposalRepo.CreateAsync(proposal, ct);

        run.ProposalsGenerated = proposals.Count;
        run.CompletedAt = DateTimeOffset.UtcNow;
        run.Status = RunStatus.Completed;
        run.Errors = JsonSerializer.Serialize(errors);

        await runRepo.UpdateAsync(run, ct);

        logger.LogInformation("Agent run {RunId} completed. Proposals: {Count}, Errors: {Errors}",
            run.Id, proposals.Count, errors.Count);
    }
}
