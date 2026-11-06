namespace CryptoMarketAnalytics.Api.Binance;

/// <summary>Settings of the public Binance market data API (section "Binance" in appsettings.json).</summary>
public sealed class BinanceOptions
{
    public const string SectionName = "Binance";

    /// <summary>Server dedicated to public market data.</summary>
    public Uri BaseUrl { get; set; } = new("https://data-api.binance.vision");

    /// <summary>Used when the primary server is unreachable or returns a server error.</summary>
    public Uri? FallbackBaseUrl { get; set; } = new("https://api.binance.com");

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
