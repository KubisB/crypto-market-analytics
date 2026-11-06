namespace CryptoMarketAnalytics.Api.Binance;

public interface IBinanceClient
{
    /// <summary>
    /// Returns closed daily candles of <paramref name="symbol"/> (e.g. BNBUSDT) for the UTC days
    /// from <paramref name="from"/> to <paramref name="to"/>, inclusive, in ascending order.
    /// The candle of the current, unfinished day is never returned.
    /// </summary>
    Task<IReadOnlyList<Kline>> GetDailyKlinesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
