using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Repositories;

/// <summary>
/// Temporary storage that keeps candles in memory until the application stops.
/// To be replaced by a persistent implementation once the storage (SQLite or files) is decided.
/// </summary>
public class InMemoryDailyCandleRepository : IDailyCandleRepository
{
    private readonly Dictionary<string, SortedDictionary<DateOnly, DailyCandle>> _candlesBySymbol = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    public Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IReadOnlyList<DailyCandle> result = _candlesBySymbol.TryGetValue(symbol, out var candles)
                ? candles.Values.Where(candle => candle.Date >= from && candle.Date <= to).ToList()
                : [];
            return Task.FromResult(result);
        }
    }

    public Task SaveAsync(string symbol, IEnumerable<DailyCandle> candles, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_candlesBySymbol.TryGetValue(symbol, out var stored))
            {
                stored = [];
                _candlesBySymbol[symbol] = stored;
            }

            foreach (var candle in candles)
            {
                stored[candle.Date] = candle;
            }
        }

        return Task.CompletedTask;
    }
}
