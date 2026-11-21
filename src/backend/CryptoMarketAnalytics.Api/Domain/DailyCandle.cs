namespace CryptoMarketAnalytics.Api.Domain;

/// <summary>One closed daily candle (OHLCV) of a trading pair. Prices are in the quote asset (USDT).</summary>
/// <param name="Date">UTC day the candle covers.</param>
/// <param name="Open">Opening price.</param>
/// <param name="High">Highest price of the day.</param>
/// <param name="Low">Lowest price of the day.</param>
/// <param name="Close">Closing price; the basis of the analysis.</param>
/// <param name="Volume">Volume in the base asset, e.g. BNB for BNBUSDT.</param>
/// <param name="QuoteVolume">Volume in the quote asset (USDT); used in the volume analysis.</param>
public record DailyCandle(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal QuoteVolume);
