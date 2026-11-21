using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Validation;

/// <summary>
/// Checks candles before they are stored, following chapter 7 of docs/requirements/wymagania-i-zakres.md.
/// Invalid candles are rejected and logged, and their day is treated as missing. That a candle starts at 00:00 UTC,
/// lasts one day and has finite numbers is checked earlier, when the Binance response is read.
/// </summary>
public class DailyCandleValidator(ILogger<DailyCandleValidator> logger)
{
    /// <summary>Valid candles of <paramref name="symbol"/>, one per day, in ascending date order.</summary>
    /// <remarks>
    /// When a day occurs more than once with different values, all its candles are rejected, because it is not known which one is right.
    /// Exact copies are kept once.
    /// </remarks>
    public IReadOnlyList<DailyCandle> Validate(string symbol, IEnumerable<DailyCandle> candles)
    {
        var valid = new List<DailyCandle>();

        foreach (var day in candles.GroupBy(candle => candle.Date).OrderBy(day => day.Key))
        {
            var versions = day.Distinct().ToList();
            if (versions.Count > 1)
            {
                logger.LogWarning("Rejected {Symbol} candles of {Date}: {Count} different candles for the same day", symbol, day.Key, versions.Count);
                continue;
            }

            var candle = versions[0];
            var reason = GetRejectionReason(candle);
            if (reason is not null)
            {
                logger.LogWarning("Rejected {Symbol} candle of {Date}: {Reason}", symbol, candle.Date, reason);
                continue;
            }

            valid.Add(candle);
        }

        return valid;
    }

    /// <summary>Why the candle is invalid, or null when it is valid.</summary>
    public static string? GetRejectionReason(DailyCandle candle)
    {
        if (candle.Open <= 0 || candle.High <= 0 || candle.Low <= 0 || candle.Close <= 0)
        {
            return "prices must be positive";
        }

        if (candle.Volume < 0 || candle.QuoteVolume < 0)
        {
            return "volume must not be negative";
        }

        if (candle.Low > Math.Min(candle.Open, candle.Close))
        {
            return "low is greater than open or close";
        }

        if (candle.High < Math.Max(candle.Open, candle.Close))
        {
            return "high is less than open or close";
        }

        return null;
    }
}
