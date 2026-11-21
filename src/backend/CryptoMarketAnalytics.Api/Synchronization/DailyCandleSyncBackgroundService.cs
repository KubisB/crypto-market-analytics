using CryptoMarketAnalytics.Api.Configuration;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Synchronization;

/// <summary>
/// Runs <see cref="DailyCandleSynchronizer"/> when the application starts and then every
/// <see cref="SynchronizationOptions.CheckInterval"/>, so the data is refreshed shortly after midnight UTC
/// and retried automatically after Binance was unavailable.
/// </summary>
public class DailyCandleSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<SynchronizationOptions> options,
    TimeProvider timeProvider,
    ILogger<DailyCandleSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Synchronisation with Binance is turned off (Synchronization:Enabled)");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = settings.CheckInterval;
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var retryAfter = await scope.ServiceProvider.GetRequiredService<DailyCandleSynchronizer>().SyncAllAsync(stoppingToken);
                if (retryAfter > delay)
                {
                    logger.LogWarning("Binance rate limit reached, next synchronisation in {RetryAfter}", retryAfter);
                    delay = retryAfter.Value;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // An unexpected error (e.g. the database) must not stop the service; the next run tries again.
                logger.LogError(exception, "Synchronisation with Binance failed unexpectedly");
            }

            try
            {
                await Task.Delay(delay, timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
