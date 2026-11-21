using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Services;

public interface IDailyCandleService
{
    /// <summary>
    /// Stored daily candles of <paramref name="symbol"/> for the UTC days from <paramref name="from"/> to <paramref name="to"/>,
    /// inclusive, with the days that have no valid candle and the days flagged for review. Binance is never called here;
    /// the data comes from the background synchronisation.
    /// </summary>
    /// <exception cref="InvalidCandleRequestException">Unsupported symbol or invalid date range.</exception>
    /// <exception cref="DataNotAvailableException">Nothing has been downloaded for the symbol yet.</exception>
    Task<CandleRange> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
