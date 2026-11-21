using System.ComponentModel.DataAnnotations;

namespace CryptoMarketAnalytics.Api.Configuration;

/// <summary>Analysed trading pairs and analysis period, section "Market" in appsettings.json. Validated at startup.</summary>
public class MarketOptions
{
    public const string SectionName = "Market";

    /// <summary>Trading pairs the API serves, e.g. BNBUSDT and BTCUSDT.</summary>
    [MinLength(1)]
    public string[] Symbols { get; set; } = [];

    /// <summary>First day of the analysis period selectable in the UI.</summary>
    public DateOnly AnalysisStartDate { get; set; }

    /// <summary>Days of data before <see cref="AnalysisStartDate"/> needed by rolling indicators (SMA200), relative volume and event study.</summary>
    [Range(0, 1000)]
    public int WarmupDays { get; set; }

    /// <summary>Earliest day the API serves: the analysis start minus the warm-up days.</summary>
    public DateOnly EarliestDate => AnalysisStartDate.AddDays(-WarmupDays);
}
