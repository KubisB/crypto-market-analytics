using System.ComponentModel.DataAnnotations;

namespace CryptoMarketAnalytics.Api.Binance.Configuration;

/// <summary>Settings of the public Binance market data API, section "Binance" in appsettings.json. Validated at startup.</summary>
public class BinanceOptions
{
    public const string SectionName = "Binance";

    /// <summary>Server dedicated to public market data.</summary>
    [Required]
    public Uri? BaseUrl { get; set; }

    /// <summary>Used when the primary server is unreachable or returns a server error. Optional.</summary>
    public Uri? FallbackBaseUrl { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
    public TimeSpan Timeout { get; set; }

    [Required]
    public string UserAgent { get; set; } = "";
}
