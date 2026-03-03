using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TechRadar.Core.Domain;

namespace TechRadar.Workers.Collectors;

public class GitHubTopicsCollector(ILogger<GitHubTopicsCollector> logger, IConfiguration config)
{
    private static readonly HttpClient Http = new();

    public async Task<List<FeedSignal>> CollectAsync(DataSource source, CancellationToken ct = default)
    {
        try
        {
            var details = JsonDocument.Parse(source.ConnectionDetails).RootElement;
            var topics = details.GetProperty("topics").EnumerateArray()
                .Select(t => t.GetString()!)
                .ToList();
            var minStars = details.TryGetProperty("minStars", out var ms) ? ms.GetInt32() : 50;
            var maxAgeDays = details.TryGetProperty("maxAgeDays", out var mad) ? mad.GetInt32() : 180;

            var since = DateTime.UtcNow.AddDays(-maxAgeDays).ToString("yyyy-MM-dd");
            var pat = config["GITHUB_PAT"];

            var seen = new HashSet<long>();
            var signals = new List<FeedSignal>();

            foreach (var topic in topics)
            {
                ct.ThrowIfCancellationRequested();

                var url = $"https://api.github.com/search/repositories" +
                          $"?q=topic:{topic}+pushed:>{since}+stars:>{minStars}+fork:false" +
                          $"&sort=stars&order=desc&per_page=30";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Add("User-Agent", "IoTTechRadar/1.0");
                req.Headers.Add("Accept", "application/vnd.github+json");
                req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                if (!string.IsNullOrWhiteSpace(pat))
                    req.Headers.Add("Authorization", $"Bearer {pat}");

                using var resp = await Http.SendAsync(req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("GitHub API returned {Status} for topic {Topic}", resp.StatusCode, topic);
                    continue;
                }

                var result = await resp.Content.ReadFromJsonAsync<GitHubSearchResult>(ct);
                if (result?.Items == null) continue;

                foreach (var repo in result.Items.Where(r => !r.Archived && seen.Add(r.Id)))
                {
                    signals.Add(new FeedSignal(
                        Title: repo.FullName,
                        Link: repo.HtmlUrl,
                        PublishedAt: repo.PushedAt,
                        Summary: repo.Description ?? string.Empty,
                        SourceId: source.Id,
                        ExternalId: repo.Id.ToString()));
                }

                // Respect GitHub rate limit: 2 second delay between topic calls
                await Task.Delay(2000, ct);
            }

            return signals;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to collect GitHub topics from source {SourceId}", source.Id);
            return [];
        }
    }

    private record GitHubSearchResult(List<GitHubRepo> Items);
    private record GitHubRepo(
        long Id,
        string FullName,
        string? Description,
        string HtmlUrl,
        bool Archived,
        DateTimeOffset PushedAt);
}
