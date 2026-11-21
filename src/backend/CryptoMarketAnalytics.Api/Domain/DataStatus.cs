namespace CryptoMarketAnalytics.Api.Domain;

/// <summary>What data the application has and how fresh it is, for the footer, the period shortcuts and the error messages.</summary>
/// <param name="LastClosedDate">Yesterday in UTC, the last day that can have a closed candle.</param>
/// <param name="AnalysisStartDate">First day of the analysis period.</param>
/// <param name="EarliestDate">First day the API serves, warm-up days included.</param>
/// <param name="Symbols">State of every configured symbol, in the order of the configuration.</param>
public record DataStatus(
    DateOnly LastClosedDate,
    DateOnly AnalysisStartDate,
    DateOnly EarliestDate,
    IReadOnlyList<SymbolSyncState> Symbols);
