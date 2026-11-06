namespace CryptoMarketAnalytics.Api.Binance;

/// <summary>One daily candle (kline) of a trading pair. Prices are in the quote asset (USDT).</summary>
/// <param name="Date">UTC day the candle covers, taken from its open time.</param>
/// <param name="Volume">Volume in the base asset (BNB for BNBUSDT, BTC for BTCUSDT).</param>
/// <param name="QuoteVolume">Volume in the quote asset (USDT); used in the volume analysis.</param>
public sealed record Kline(
    DateOnly Date,
    DateTimeOffset OpenTime,
    DateTimeOffset CloseTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal QuoteVolume,
    long TradeCount);
