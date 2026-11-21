using System.ComponentModel.DataAnnotations;

namespace CryptoMarketAnalytics.Api.Configuration;

/// <summary>Background synchronisation of candles with Binance, section "Synchronization" in appsettings.json. Validated at startup.</summary>
public class SynchronizationOptions
{
    public const string SectionName = "Synchronization";

    /// <summary>Turns the background synchronisation on; tests turn it off and fill the storage themselves.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// How often the application checks whether yesterday's candle is stored. Binance is called only when it is not,
    /// so in practice the data is downloaded once a day, shortly after midnight UTC plus <see cref="ClosedCandleDelay"/>,
    /// and again after a failed attempt.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan CheckInterval { get; set; }

    /// <summary>
    /// How long after midnight UTC the candle of the previous day is downloaded, so Binance has finished it.
    /// The API still treats the day as closed from midnight; only the download waits.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00", "01:00:00")]
    public TimeSpan ClosedCandleDelay { get; set; }
}
