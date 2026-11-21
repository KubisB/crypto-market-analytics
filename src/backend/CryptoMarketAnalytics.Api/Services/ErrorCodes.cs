namespace CryptoMarketAnalytics.Api.Services;

/// <summary>
/// Values of the <c>code</c> field of error responses. They are part of the API contract: the frontend maps them
/// to the Polish messages from docs/requirements/teksty-interfejsu.md, so a code must not be renamed.
/// </summary>
public static class ErrorCodes
{
    /// <summary>400: a required query parameter is missing or has a wrong format, e.g. a date that is not yyyy-MM-dd.</summary>
    public const string InvalidParameter = "invalid-parameter";

    /// <summary>400: the symbol is not one of <c>Market:Symbols</c>.</summary>
    public const string UnsupportedSymbol = "unsupported-symbol";

    /// <summary>400: <c>from</c> is later than <c>to</c>.</summary>
    public const string InvalidDateRange = "invalid-date-range";

    /// <summary>400: <c>from</c> is earlier than the earliest day the API serves.</summary>
    public const string DateBeforeEarliest = "date-before-earliest";

    /// <summary>400: <c>to</c> is today or later; today's candle is not closed yet.</summary>
    public const string DateNotClosed = "date-not-closed";

    /// <summary>503: no data has been downloaded from Binance yet, e.g. right after the first start or when Binance is unavailable.</summary>
    public const string DataNotAvailable = "data-not-available";
}
