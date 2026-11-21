using CryptoMarketAnalytics.Api.Binance.Models;

namespace CryptoMarketAnalytics.Api.Binance.Mapping;

/// <summary>One page of the klines response after reading it.</summary>
/// <param name="Klines">Klines that were read correctly.</param>
/// <param name="Rejected">Why the other klines were rejected; a rejected day is treated as missing.</param>
/// <param name="RowCount">Number of klines Binance returned, rejected ones included; used for paging.</param>
/// <param name="LastOpenTime">Open time of the last kline returned; the next page starts one day later.</param>
public record BinanceKlinePage(
    IReadOnlyList<BinanceKline> Klines,
    IReadOnlyList<string> Rejected,
    int RowCount,
    DateTimeOffset? LastOpenTime);
