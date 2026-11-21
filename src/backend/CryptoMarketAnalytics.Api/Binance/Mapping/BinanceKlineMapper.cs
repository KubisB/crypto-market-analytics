using System.Globalization;
using System.Text.Json;
using CryptoMarketAnalytics.Api.Binance.Models;
using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Binance.Mapping;

public static class BinanceKlineMapper
{
    /// <summary>
    /// Reads the klines response: an array of candles, each an array of 12 values. Used fields:
    /// 0 open time (ms), 1–4 open, high, low, close, 5 base asset volume, 6 close time (ms),
    /// 7 quote asset volume, 8 number of trades. Prices and volumes are strings, times are numbers.
    /// </summary>
    /// <exception cref="FormatException">The response does not have the expected shape.</exception>
    public static IReadOnlyList<BinanceKline> FromJson(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException($"Expected a JSON array of klines, got {root.ValueKind}.");
        }

        var klines = new List<BinanceKline>(root.GetArrayLength());
        foreach (var kline in root.EnumerateArray())
        {
            if (kline.ValueKind != JsonValueKind.Array || kline.GetArrayLength() < 9)
            {
                throw new FormatException("Expected a kline as an array of at least 9 values.");
            }

            klines.Add(new BinanceKline(
                OpenTime: DateTimeOffset.FromUnixTimeMilliseconds(kline[0].GetInt64()),
                Open: ParseDecimal(kline[1]),
                High: ParseDecimal(kline[2]),
                Low: ParseDecimal(kline[3]),
                Close: ParseDecimal(kline[4]),
                Volume: ParseDecimal(kline[5]),
                CloseTime: DateTimeOffset.FromUnixTimeMilliseconds(kline[6].GetInt64()),
                QuoteAssetVolume: ParseDecimal(kline[7]),
                NumberOfTrades: kline[8].GetInt64()));
        }

        return klines;
    }

    public static DailyCandle ToDailyCandle(BinanceKline kline) => new(
        Date: DateOnly.FromDateTime(kline.OpenTime.UtcDateTime),
        Open: kline.Open,
        High: kline.High,
        Low: kline.Low,
        Close: kline.Close,
        Volume: kline.Volume,
        QuoteVolume: kline.QuoteAssetVolume);

    // Binance uses a dot as the decimal separator, so parsing must not depend on the system culture.
    private static decimal ParseDecimal(JsonElement value) =>
        decimal.Parse(
            value.GetString() ?? throw new FormatException("Missing numeric value."),
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
}
