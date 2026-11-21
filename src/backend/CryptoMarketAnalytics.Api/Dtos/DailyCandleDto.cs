using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Dtos;

/// <summary>Daily candle returned by the API. Prices are in USDT.</summary>
/// <param name="Date">UTC day of the candle.</param>
/// <param name="Open">Opening price.</param>
/// <param name="High">Highest price of the day.</param>
/// <param name="Low">Lowest price of the day.</param>
/// <param name="Close">Closing price; the basis of the analysis.</param>
/// <param name="Volume">Volume in the base asset, e.g. BNB for BNBUSDT.</param>
/// <param name="QuoteVolume">Volume in USDT.</param>
/// <param name="FlaggedForReview">
/// The close changed by more than <c>Market:LargePriceChangeThreshold</c> (50%) from the previous day.
/// The candle is kept; the flag only marks it for a manual check.
/// </param>
public record DailyCandleDto(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal QuoteVolume,
    bool FlaggedForReview)
{
    public static DailyCandleDto FromDomain(DailyCandle candle, bool flaggedForReview = false) => new(
        candle.Date, candle.Open, candle.High, candle.Low, candle.Close, candle.Volume, candle.QuoteVolume, flaggedForReview);
}
