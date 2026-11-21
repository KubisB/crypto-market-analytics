using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Repositories;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Services;

public class DailyCandleService(
    IDailyCandleRepository repository,
    IBinanceClient binanceClient,
    IOptions<MarketOptions> options,
    TimeProvider timeProvider) : IDailyCandleService
{
    public async Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        if (!settings.Symbols.Contains(normalizedSymbol, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidCandleRequestException($"Unsupported symbol '{symbol}'. Supported symbols: {string.Join(", ", settings.Symbols)}.");
        }

        if (from > to)
        {
            throw new InvalidCandleRequestException("'from' must not be later than 'to'.");
        }

        if (from < settings.EarliestDate)
        {
            throw new InvalidCandleRequestException($"'from' must not be earlier than {settings.EarliestDate:yyyy-MM-dd}.");
        }

        var yesterday = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(-1);
        var lastDay = to < yesterday ? to : yesterday;
        if (from > lastDay)
        {
            return [];
        }

        var stored = await repository.GetAsync(normalizedSymbol, from, lastDay, cancellationToken);
        var expectedDays = lastDay.DayNumber - from.DayNumber + 1;
        if (stored.Count == expectedDays)
        {
            return stored;
        }

        // Until the daily synchronisation exists, missing days are fetched from Binance on demand.
        // Days that Binance itself does not have (gaps) are requested again on every call.
        var fetched = await binanceClient.GetDailyCandlesAsync(normalizedSymbol, from, lastDay, cancellationToken);
        await repository.SaveAsync(normalizedSymbol, fetched, cancellationToken);
        return await repository.GetAsync(normalizedSymbol, from, lastDay, cancellationToken);
    }
}
