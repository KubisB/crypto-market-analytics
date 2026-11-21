using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Repositories;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Services;

public class DailyCandleService(
    IDailyCandleRepository candles,
    ISyncStateRepository states,
    IOptions<MarketOptions> options,
    TimeProvider timeProvider) : IDailyCandleService
{
    public async Task<CandleRange> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var yesterday = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(-1);

        if (!settings.Symbols.Contains(normalizedSymbol, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidCandleRequestException(ErrorCodes.UnsupportedSymbol,
                $"Unsupported symbol '{symbol}'. Supported symbols: {string.Join(", ", settings.Symbols)}.");
        }

        if (from > to)
        {
            throw new InvalidCandleRequestException(ErrorCodes.InvalidDateRange, "'from' must not be later than 'to'.");
        }

        if (from < settings.EarliestDate)
        {
            throw new InvalidCandleRequestException(ErrorCodes.DateBeforeEarliest,
                $"'from' must not be earlier than {settings.EarliestDate:yyyy-MM-dd}.");
        }

        if (to > yesterday)
        {
            throw new InvalidCandleRequestException(ErrorCodes.DateNotClosed,
                $"'to' must not be later than {yesterday:yyyy-MM-dd}, the last closed day; today's candle is not closed yet.");
        }

        var state = await states.GetAsync(normalizedSymbol, cancellationToken);
        if (state?.LastDate is null)
        {
            throw new DataNotAvailableException(state?.LastError is { } error
                ? $"No data of {normalizedSymbol} has been downloaded yet. Last attempt failed: {error}"
                : $"No data of {normalizedSymbol} has been downloaded yet.");
        }

        // The day before the range is read too, so that the first day can be compared with the previous close.
        var stored = await candles.GetAsync(normalizedSymbol, from.AddDays(-1), to, cancellationToken);
        var flagged = LargePriceChange.FindDates(stored, settings.LargePriceChangeThreshold);
        var inRange = stored.Where(candle => candle.Date >= from).ToList();

        // Completeness is checked for the whole requested range, its first and last day included (chapter 7).
        // Days after the last synchronised day are missing as well; the data status says whether the data is up to date.
        var storedDays = inRange.Select(candle => candle.Date).ToHashSet();
        var missingDays = Enumerable.Range(from.DayNumber, to.DayNumber - from.DayNumber + 1)
            .Select(DateOnly.FromDayNumber)
            .Where(day => !storedDays.Contains(day))
            .ToList();

        return new CandleRange(normalizedSymbol, from, to, inRange, missingDays,
            flagged.Where(day => day >= from).ToHashSet());
    }
}
