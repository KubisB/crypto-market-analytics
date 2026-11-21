using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Dtos;

/// <summary>Candles of one symbol for a range of UTC days.</summary>
/// <param name="Symbol">Trading pair, e.g. BNBUSDT.</param>
/// <param name="From">First requested day.</param>
/// <param name="To">Last requested day, inclusive.</param>
/// <param name="Candles">Candles in ascending date order.</param>
/// <param name="MissingDays">
/// Days of the range without a valid candle: gaps at Binance, rejected records, or days not synchronised yet.
/// They are not filled in; price changes across them are left out of daily statistics.
/// </param>
public record CandleRangeDto(
    string Symbol,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<DailyCandleDto> Candles,
    IReadOnlyList<DateOnly> MissingDays)
{
    public static CandleRangeDto FromDomain(CandleRange range) => new(
        range.Symbol,
        range.From,
        range.To,
        range.Candles.Select(candle => DailyCandleDto.FromDomain(candle, range.FlaggedDays.Contains(candle.Date))).ToList(),
        range.MissingDays);
}
