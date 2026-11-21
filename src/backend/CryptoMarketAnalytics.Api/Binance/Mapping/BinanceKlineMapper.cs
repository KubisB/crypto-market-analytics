using System.Globalization;
using System.Text.Json;
using CryptoMarketAnalytics.Api.Binance.Models;
using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Binance.Mapping;

public static class BinanceKlineMapper
{
    private static readonly TimeSpan KlineDuration = TimeSpan.FromDays(1) - TimeSpan.FromMilliseconds(1);

    /// <summary>
    /// Reads the klines response: an array of candles, each an array of 12 values. Used fields:
    /// 0 open time (ms), 1–4 open, high, low, close, 5 base asset volume, 6 close time (ms),
    /// 7 quote asset volume, 8 number of trades. Prices and volumes are strings, times are numbers.
    /// A kline that cannot be read, or that does not cover exactly one UTC day from 00:00, is rejected
    /// on its own and reported in <see cref="BinanceKlinePage.Rejected"/>; the rest of the page is kept.
    /// </summary>
    /// <exception cref="FormatException">The response is not an array.</exception>
    public static BinanceKlinePage FromJson(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException($"Expected a JSON array of klines, got {root.ValueKind}.");
        }

        var klines = new List<BinanceKline>(root.GetArrayLength());
        var rejected = new List<string>();
        DateTimeOffset? lastOpenTime = null;

        foreach (var row in root.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() < 9)
            {
                rejected.Add("expected an array of at least 9 values");
                continue;
            }

            if (row[0].ValueKind != JsonValueKind.Number || !row[0].TryGetInt64(out var openTimeMs))
            {
                rejected.Add("missing open time");
                continue;
            }

            var openTime = DateTimeOffset.FromUnixTimeMilliseconds(openTimeMs);
            lastOpenTime = openTime;

            if (!TryParse(row, openTime, out var kline, out var reason))
            {
                rejected.Add($"{openTime:yyyy-MM-dd}: {reason}");
                continue;
            }

            if (openTime.TimeOfDay != TimeSpan.Zero || kline.CloseTime - kline.OpenTime != KlineDuration)
            {
                rejected.Add($"{openTime:yyyy-MM-dd}: does not cover one UTC day from 00:00 (open {openTime:O}, close {kline.CloseTime:O})");
                continue;
            }

            klines.Add(kline);
        }

        return new BinanceKlinePage(klines, rejected, root.GetArrayLength(), lastOpenTime);
    }

    public static DailyCandle ToDailyCandle(BinanceKline kline) => new(
        Date: DateOnly.FromDateTime(kline.OpenTime.UtcDateTime),
        Open: kline.Open,
        High: kline.High,
        Low: kline.Low,
        Close: kline.Close,
        Volume: kline.Volume,
        QuoteVolume: kline.QuoteAssetVolume);

    /// <summary>Status of the symbol from <c>GET /api/v3/exchangeInfo?symbol=…</c>, e.g. <c>TRADING</c> or <c>BREAK</c>.</summary>
    /// <exception cref="FormatException">The response does not contain the symbol status.</exception>
    public static string StatusFromExchangeInfo(JsonElement root, string symbol)
    {
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("symbols", out var symbols)
            && symbols.ValueKind == JsonValueKind.Array)
        {
            foreach (var info in symbols.EnumerateArray())
            {
                if (info.ValueKind == JsonValueKind.Object
                    && info.TryGetProperty("symbol", out var name) && name.ValueKind == JsonValueKind.String
                    && string.Equals(name.GetString(), symbol, StringComparison.OrdinalIgnoreCase)
                    && info.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.String)
                {
                    return status.GetString()!;
                }
            }
        }

        throw new FormatException($"exchangeInfo does not contain the status of {symbol}.");
    }

    private static bool TryParse(JsonElement row, DateTimeOffset openTime, out BinanceKline kline, out string reason)
    {
        kline = null!;
        try
        {
            if (row[6].ValueKind != JsonValueKind.Number || row[8].ValueKind != JsonValueKind.Number)
            {
                reason = "close time or number of trades is not a number";
                return false;
            }

            kline = new BinanceKline(
                OpenTime: openTime,
                Open: ParseDecimal(row[1]),
                High: ParseDecimal(row[2]),
                Low: ParseDecimal(row[3]),
                Close: ParseDecimal(row[4]),
                Volume: ParseDecimal(row[5]),
                CloseTime: DateTimeOffset.FromUnixTimeMilliseconds(row[6].GetInt64()),
                QuoteAssetVolume: ParseDecimal(row[7]),
                NumberOfTrades: row[8].GetInt64());
            reason = "";
            return true;
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or InvalidOperationException or ArgumentOutOfRangeException)
        {
            reason = exception.Message;
            return false;
        }
    }

    // Binance uses a dot as the decimal separator, so parsing must not depend on the system culture.
    // Values such as "NaN" or "Infinity" fail to parse, so every accepted number is finite.
    private static decimal ParseDecimal(JsonElement value) =>
        decimal.Parse(
            value.ValueKind == JsonValueKind.String ? value.GetString()! : throw new FormatException("Missing numeric value."),
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
}
