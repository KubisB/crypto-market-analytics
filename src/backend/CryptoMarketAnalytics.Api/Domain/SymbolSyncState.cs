namespace CryptoMarketAnalytics.Api.Domain;

/// <summary>What has been downloaded from Binance for one symbol.</summary>
/// <param name="Symbol">Trading pair, e.g. BNBUSDT.</param>
/// <param name="FirstDate">First day covered by the downloaded data; null before the first successful download.</param>
/// <param name="LastDate">
/// Last day covered by the downloaded data. Every day from <paramref name="FirstDate"/> to here was requested from Binance,
/// so a day without a stored candle in this range is a known gap and is not requested again.
/// </param>
/// <param name="LastSyncedAt">When the last successful download finished.</param>
/// <param name="LastAttemptAt">When the last download was attempted, successful or not.</param>
/// <param name="LastError">Why the last attempt failed; null after a success.</param>
/// <param name="ExchangeStatus">Status of the pair from exchangeInfo, e.g. TRADING; null if not checked yet.</param>
public record SymbolSyncState(
    string Symbol,
    DateOnly? FirstDate = null,
    DateOnly? LastDate = null,
    DateTimeOffset? LastSyncedAt = null,
    DateTimeOffset? LastAttemptAt = null,
    string? LastError = null,
    string? ExchangeStatus = null);
