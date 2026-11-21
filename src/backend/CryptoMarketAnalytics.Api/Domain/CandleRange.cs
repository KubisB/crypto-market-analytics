namespace CryptoMarketAnalytics.Api.Domain;

/// <summary>Stored candles of one symbol for a requested range of UTC days, with the completeness of the range.</summary>
/// <param name="Symbol">Trading pair, e.g. BNBUSDT.</param>
/// <param name="From">First requested day.</param>
/// <param name="To">Last requested day, inclusive.</param>
/// <param name="Candles">Candles in ascending date order.</param>
/// <param name="MissingDays">Days of the range without a valid candle; they are not filled in.</param>
/// <param name="FlaggedDays">Days whose close changed by more than the threshold from the previous day, for manual review.</param>
public record CandleRange(
    string Symbol,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<DailyCandle> Candles,
    IReadOnlyList<DateOnly> MissingDays,
    IReadOnlySet<DateOnly> FlaggedDays);
