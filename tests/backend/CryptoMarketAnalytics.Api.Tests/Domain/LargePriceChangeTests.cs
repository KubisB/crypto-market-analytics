using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Tests.Domain;

public class LargePriceChangeTests
{
    private static readonly DateOnly Day = new(2026, 1, 1);

    [Fact]
    public void FlagsChangesAboveThresholdInBothDirections()
    {
        var candles = Closes(100m, 151m, 75m, 76m);

        var flagged = LargePriceChange.FindDates(candles, 0.5m);

        // 100 → 151 is +51%, 151 → 75 is −50.3%, 75 → 76 is +1.3%.
        Assert.Equal([Day.AddDays(1), Day.AddDays(2)], flagged.Order());
    }

    [Fact]
    public void ChangeOfExactlyTheThreshold_IsNotFlagged()
    {
        Assert.Empty(LargePriceChange.FindDates(Closes(100m, 150m), 0.5m));
    }

    [Fact]
    public void DayAfterGap_IsNotComparedWithOlderClose()
    {
        var candles = new[] { Candle(Day, 100m), Candle(Day.AddDays(2), 200m) };

        Assert.Empty(LargePriceChange.FindDates(candles, 0.5m));
    }

    private static DailyCandle[] Closes(params decimal[] closes) =>
        closes.Select((close, i) => Candle(Day.AddDays(i), close)).ToArray();

    private static DailyCandle Candle(DateOnly date, decimal close) => new(date, close, close, close, close, 1m, close);
}
