using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Repositories;

/// <summary>Local storage of daily candles, so that the API does not depend on Binance for every request.</summary>
public interface IDailyCandleRepository
{
    /// <summary>Stored candles of <paramref name="symbol"/> for the days from <paramref name="from"/> to <paramref name="to"/>, inclusive, in ascending order.</summary>
    Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>Stores the candles; a candle of a day that is already stored replaces it.</summary>
    Task SaveAsync(string symbol, IReadOnlyList<DailyCandle> candles, CancellationToken cancellationToken = default);
}
