using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Repositories;
using CryptoMarketAnalytics.Api.Validation;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Synchronization;

/// <summary>
/// Brings the stored candles up to yesterday, the last closed day (chapters 7 and 10.3 of the requirements).
/// The first run downloads the whole history from <see cref="MarketOptions.EarliestDate"/>; later runs download only the new days.
/// Binance is called only when data is missing, so the method can be run as often as needed.
/// </summary>
public class DailyCandleSynchronizer(
    IBinanceClient binanceClient,
    IDailyCandleRepository candles,
    ISyncStateRepository states,
    DailyCandleValidator validator,
    IOptions<MarketOptions> marketOptions,
    IOptions<SynchronizationOptions> synchronizationOptions,
    TimeProvider timeProvider,
    ILogger<DailyCandleSynchronizer> logger)
{
    /// <summary>Status of an active pair in exchangeInfo.</summary>
    public const string TradingStatus = "TRADING";

    /// <summary>Synchronises every configured symbol.</summary>
    /// <returns>How long to wait before the next attempt when Binance asked to slow down (429 or 418), otherwise null.</returns>
    public async Task<TimeSpan?> SyncAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var symbol in marketOptions.Value.Symbols)
        {
            try
            {
                await SyncAsync(symbol.Trim().ToUpperInvariant(), cancellationToken);
            }
            catch (BinanceApiException exception) when (exception.IsRateLimited)
            {
                // Further requests would only extend the IP ban, so the remaining symbols wait for the next run.
                return exception.RetryAfter ?? TimeSpan.FromMinutes(5);
            }
            catch (BinanceApiException)
            {
                // Already logged and recorded in the sync state; the other symbols are still synchronised.
            }
        }

        return null;
    }

    /// <summary>
    /// Synchronises one symbol and records the outcome in its <see cref="SymbolSyncState"/>.
    /// <see cref="SymbolSyncState.LastDate"/> becomes the last stored candle, so days missing at the end of the range
    /// are downloaded again on the next run; days missing in the middle stay missing.
    /// </summary>
    /// <exception cref="BinanceApiException">
    /// Binance failed or could not be reached; the error is stored in the sync state before it is rethrown.
    /// </exception>
    public async Task SyncAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        // Yesterday, once Binance has had ClosedCandleDelay after midnight UTC to finish its candle.
        var yesterday = DateOnly.FromDateTime((now - synchronizationOptions.Value.ClosedCandleDelay).UtcDateTime).AddDays(-1);
        var earliest = marketOptions.Value.EarliestDate;
        var state = await states.GetAsync(symbol, cancellationToken) ?? new SymbolSyncState(symbol);

        // When the earliest date was moved back in the configuration, the whole range is downloaded again.
        var coversStart = state.FirstDate is { } first && first <= earliest;
        if (coversStart && state.LastDate >= yesterday)
        {
            return;
        }

        var from = coversStart && state.LastDate is { } last ? last.AddDays(1) : earliest;

        try
        {
            var status = await binanceClient.GetSymbolStatusAsync(symbol, cancellationToken);
            state = state with { ExchangeStatus = status };
            if (!string.Equals(status, TradingStatus, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("{Symbol} has status {Status} on Binance instead of {Trading}; it is not synchronised, stored data is still served",
                    symbol, status, TradingStatus);
                await states.SaveAsync(state with { LastAttemptAt = now, LastError = $"Pair status on Binance is {status}." }, cancellationToken);
                return;
            }

            var downloaded = await binanceClient.GetDailyCandlesAsync(symbol, from, yesterday, cancellationToken);
            var valid = validator.Validate(symbol, downloaded);
            await candles.SaveAsync(symbol, valid, cancellationToken);

            // The validator returns the candles sorted by date. An empty response keeps the previous last date.
            var lastDate = valid.Count > 0 && (state.LastDate is null || valid[^1].Date > state.LastDate)
                ? valid[^1].Date
                : state.LastDate;
            var lastInRange = lastDate is { } date && date >= from ? date : from.AddDays(-1);

            var gaps = lastInRange.DayNumber - from.DayNumber + 1 - valid.Count;
            if (gaps > 0)
            {
                logger.LogWarning("{Symbol}: {Missing} days from {From} to {To} have no valid candle and stay missing",
                    symbol, gaps, from, lastInRange);
            }

            var pending = yesterday.DayNumber - lastInRange.DayNumber;
            if (pending > 0)
            {
                logger.LogWarning("{Symbol}: no valid candle for the last {Pending} days up to {To}; they are downloaded again on the next run",
                    symbol, pending, yesterday);
            }

            await states.SaveAsync(state with
            {
                FirstDate = coversStart ? state.FirstDate : earliest,
                LastDate = lastDate,
                LastSyncedAt = now,
                LastAttemptAt = now,
                LastError = null,
            }, cancellationToken);
            logger.LogInformation("{Symbol} synchronised from {From} to {To}: {Count} candles stored", symbol, from, yesterday, valid.Count);
        }
        catch (BinanceApiException exception)
        {
            logger.LogWarning(exception, "Synchronisation of {Symbol} failed; stored data is still served", symbol);
            await states.SaveAsync(state with { LastAttemptAt = now, LastError = exception.Message }, cancellationToken);
            throw;
        }
    }
}
