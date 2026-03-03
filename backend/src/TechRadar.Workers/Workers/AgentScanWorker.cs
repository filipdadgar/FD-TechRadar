using System.Threading.Channels;
using Cronos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechRadar.Workers.Services;

namespace TechRadar.Workers.Workers;

public class AgentScanWorker(
    IServiceScopeFactory scopeFactory,
    Channel<bool> triggerChannel,
    IConfiguration config,
    ILogger<AgentScanWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cronExpr = config["AGENTS_SCHEDULE_CRON"] ?? "0 3 * * *";
        var cron = CronExpression.Parse(cronExpr, CronFormat.Standard);

        logger.LogInformation("AgentScanWorker started. Schedule: {Cron}", cronExpr);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = cron.GetNextOccurrence(now, TimeZoneInfo.Utc);

            if (next == null)
            {
                logger.LogWarning("Could not compute next cron occurrence for '{Cron}'. Retrying in 1h.", cronExpr);
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                continue;
            }

            var delay = next.Value - now;
            logger.LogInformation("Next scheduled scan at {Next} (in {Delay:hh\\:mm\\:ss})", next, delay);

            // Wait for scheduled time OR a manual trigger — whichever comes first
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var manualTriggerTask = triggerChannel.Reader.WaitToReadAsync(cts.Token).AsTask();
            var delayTask = Task.Delay(delay, cts.Token);

            var completed = await Task.WhenAny(delayTask, manualTriggerTask);
            cts.Cancel(); // clean up the other task

            string triggerType;
            if (completed == manualTriggerTask && !stoppingToken.IsCancellationRequested)
            {
                while (triggerChannel.Reader.TryRead(out _)) { } // drain channel
                triggerType = "manual";
            }
            else
            {
                triggerType = "scheduled";
            }

            if (stoppingToken.IsCancellationRequested) break;

            logger.LogInformation("Starting agent scan (trigger: {Trigger})", triggerType);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<AgentScanService>();
                await service.RunAsync(triggerType, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Agent scan failed unexpectedly");
            }
        }
    }
}
