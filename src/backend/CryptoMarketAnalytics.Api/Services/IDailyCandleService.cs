using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Services;

public interface IDailyCandleService
{
    /// <summary>
    /// Closed daily candles of <paramref name="symbol"/> for the UTC days from <paramref name="from"/> to <paramref name="to"/>,
    /// inclusive. Days after yesterday are ignored, because today's candle is not closed yet.
    /// </summary>
    /// <exception cref="InvalidCandleRequestException">Unsupported symbol or invalid date range.</exception>
    Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
