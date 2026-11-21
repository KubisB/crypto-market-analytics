namespace CryptoMarketAnalytics.Api.Domain;

/// <summary>
/// Daily price changes large enough to need a manual check (chapter 7 of the requirements).
/// They are only flagged: the candle stays in the data and in the statistics; the event study leaves such days out of its estimation window.
/// </summary>
public static class LargePriceChange
{
    /// <summary>
    /// Days whose closing price differs from the close of the previous calendar day by more than
    /// <paramref name="threshold"/> (a fraction, 0.5 = 50%). A day after a gap has no previous close and is never flagged.
    /// </summary>
    /// <param name="candles">Candles of one symbol in ascending date order.</param>
    /// <param name="threshold">Maximum accepted absolute change, as a fraction.</param>
    public static IReadOnlySet<DateOnly> FindDates(IReadOnlyList<DailyCandle> candles, decimal threshold)
    {
        var flagged = new HashSet<DateOnly>();
        for (var i = 1; i < candles.Count; i++)
        {
            var previous = candles[i - 1];
            var current = candles[i];
            if (current.Date == previous.Date.AddDays(1) && Math.Abs(current.Close / previous.Close - 1) > threshold)
            {
                flagged.Add(current.Date);
            }
        }

        return flagged;
    }
}
