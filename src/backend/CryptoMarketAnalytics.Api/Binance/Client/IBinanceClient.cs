using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Binance.Client;

public interface IBinanceClient
{
    /// <summary>
    /// Returns closed daily candles of <paramref name="symbol"/> (e.g. BNBUSDT) for the UTC days
    /// from <paramref name="from"/> to <paramref name="to"/>, inclusive, in ascending order.
    /// The candle of the current, unfinished day is never returned. Klines that cannot be read or do not
    /// cover one UTC day are skipped and logged; their values are checked later by the candle validator.
    /// </summary>
    /// <exception cref="BinanceApiException">Binance is unavailable, rate-limited or returned an unexpected response.</exception>
    Task<IReadOnlyList<DailyCandle>> GetDailyCandlesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>Trading status of <paramref name="symbol"/>; <c>TRADING</c> for an active pair.</summary>
    /// <exception cref="BinanceApiException">Binance is unavailable or does not know the symbol.</exception>
    Task<string> GetSymbolStatusAsync(string symbol, CancellationToken cancellationToken = default);
}
