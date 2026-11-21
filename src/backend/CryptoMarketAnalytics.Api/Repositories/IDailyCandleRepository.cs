using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Repositories;

/// <summary>Locally stored daily candles, the source the application reads from.</summary>
public interface IDailyCandleRepository
{
    /// <summary>Stored candles of <paramref name="symbol"/> for the days from <paramref name="from"/> to <paramref name="to"/>, inclusive, in ascending order.</summary>
    Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>Adds candles of <paramref name="symbol"/>; a candle for an already stored day replaces it.</summary>
    Task SaveAsync(string symbol, IEnumerable<DailyCandle> candles, CancellationToken cancellationToken = default);
}
