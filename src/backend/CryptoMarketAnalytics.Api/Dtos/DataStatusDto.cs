using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Dtos;

/// <summary>What data the application has and how fresh it is. All dates are UTC days, all times are UTC.</summary>
/// <param name="LastClosedDate">Yesterday, the last day with a closed candle; the period shortcuts count back from it.</param>
/// <param name="AnalysisStartDate">First day the user can select (1.01.2023).</param>
/// <param name="EarliestDate">First day served by <c>/api/candles</c>, the warm-up days for rolling indicators included.</param>
/// <param name="Symbols">State of each trading pair.</param>
public record DataStatusDto(
    DateOnly LastClosedDate,
    DateOnly AnalysisStartDate,
    DateOnly EarliestDate,
    IReadOnlyList<SymbolStatusDto> Symbols)
{
    public static DataStatusDto FromDomain(DataStatus status) => new(
        status.LastClosedDate,
        status.AnalysisStartDate,
        status.EarliestDate,
        status.Symbols.Select(symbol => SymbolStatusDto.FromDomain(symbol, status.LastClosedDate)).ToList());
}

/// <summary>Synchronisation state of one trading pair.</summary>
/// <param name="Symbol">Trading pair, e.g. BNBUSDT.</param>
/// <param name="FirstDate">First synchronised day; null until the first download succeeds.</param>
/// <param name="LastDate">Last synchronised day ("Dane do dnia {data}" in the footer); null until the first download succeeds.</param>
/// <param name="LastSyncedAt">When data was last downloaded successfully ("ostatnie pobranie {data} o {godzina} UTC").</param>
/// <param name="LastAttemptAt">When the last download was attempted, successful or not.</param>
/// <param name="LastError">Why the last attempt failed, in English, for developers; null after a success.</param>
/// <param name="ExchangeStatus">Status of the pair on Binance, <c>TRADING</c> for an active pair; null if not checked yet.</param>
/// <param name="IsUpToDate">
/// True when the data reaches <see cref="DataStatusDto.LastClosedDate"/>. False with a <paramref name="LastSyncedAt"/> means
/// stored data is shown because Binance could not be reached; false without it means there is no data yet.
/// </param>
public record SymbolStatusDto(
    string Symbol,
    DateOnly? FirstDate,
    DateOnly? LastDate,
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset? LastAttemptAt,
    string? LastError,
    string? ExchangeStatus,
    bool IsUpToDate)
{
    public static SymbolStatusDto FromDomain(SymbolSyncState state, DateOnly lastClosedDate) => new(
        state.Symbol,
        state.FirstDate,
        state.LastDate,
        state.LastSyncedAt,
        state.LastAttemptAt,
        state.LastError,
        state.ExchangeStatus,
        state.LastDate >= lastClosedDate);
}
