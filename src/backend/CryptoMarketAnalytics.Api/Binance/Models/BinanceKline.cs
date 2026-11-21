namespace CryptoMarketAnalytics.Api.Binance.Models;

/// <summary>
/// One kline as returned by <c>GET /api/v3/klines</c>. Binance sends it as an array of 12 values;
/// only the fields used by the project are kept. Field names follow the Binance documentation.
/// </summary>
public record BinanceKline(
    DateTimeOffset OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    DateTimeOffset CloseTime,
    decimal QuoteAssetVolume,
    long NumberOfTrades);
