using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using TechRadar.Core.Domain;

namespace TechRadar.Workers.Collectors;

public record FeedSignal(
    string Title,
    string Link,
    DateTimeOffset PublishedAt,
    string Summary,
    Guid SourceId,
    string ExternalId);

public class RssFeedCollector(ILogger<RssFeedCollector> logger)
{
    public async Task<List<FeedSignal>> CollectAsync(DataSource source, CancellationToken ct = default)
    {
        try
        {
            var url = System.Text.Json.JsonDocument.Parse(source.ConnectionDetails)
                .RootElement.GetProperty("url").GetString()!;

            var feed = await FeedReader.ReadAsync(url, ct);
            var cutoff = DateTimeOffset.UtcNow.AddDays(-90);

            return feed.Items
                .Where(i => i.PublishingDate == null || i.PublishingDate.Value >= cutoff.DateTime)
                .Select(i => new FeedSignal(
                    Title: i.Title ?? string.Empty,
                    Link: i.Link ?? string.Empty,
                    PublishedAt: i.PublishingDate.HasValue
                        ? new DateTimeOffset(i.PublishingDate.Value, TimeSpan.Zero)
                        : DateTimeOffset.UtcNow,
                    Summary: System.Text.RegularExpressions.Regex.Replace(
                        i.Description ?? string.Empty, "<.*?>", string.Empty),
                    SourceId: source.Id,
                    ExternalId: i.Id ?? i.Link ?? Guid.NewGuid().ToString()))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to collect RSS feed from source {SourceId} ({SourceName})",
                source.Id, source.Name);
            return [];
        }
    }
}
