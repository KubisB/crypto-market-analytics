using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Binance;

/// <summary>
/// Client of the public Binance REST API (<c>GET /api/v3/klines</c>), as described in chapter 10
/// of docs/requirements/wymagania-i-zakres.md. No account or API key is needed.
/// </summary>
public sealed class BinanceClient(
    HttpClient httpClient,
    IOptions<BinanceOptions> options,
    TimeProvider timeProvider,
    ILogger<BinanceClient> logger) : IBinanceClient
{
    /// <summary>Maximum number of candles Binance returns per request.</summary>
    public const int PageSize = 1000;

    private const long DayMilliseconds = 86_400_000;

    public async Task<IReadOnlyList<Kline>> GetDailyKlinesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        if (from > to)
        {
            throw new ArgumentException("'from' must not be later than 'to'.", nameof(from));
        }

        var startTime = ToUnixMilliseconds(from);
        var endTime = ToUnixMilliseconds(to.AddDays(1)) - 1;
        var now = timeProvider.GetUtcNow();
        var klines = new List<Kline>();

        // Binance returns at most 1000 candles per request; the next page starts one day after
        // the open time of the last candle received, until a page is shorter than the limit.
        while (startTime <= endTime)
        {
            var page = await GetPageAsync(symbol, startTime, endTime, cancellationToken);

            // The API also returns the current, unfinished day; only closed candles are kept.
            klines.AddRange(page.Where(kline => kline.CloseTime <= now));

            if (page.Count < PageSize)
            {
                break;
            }

            startTime = page[^1].OpenTime.ToUnixTimeMilliseconds() + DayMilliseconds;
        }

        return klines;
    }

    private async Task<List<Kline>> GetPageAsync(string symbol, long startTime, long endTime, CancellationToken cancellationToken)
    {
        var path = $"api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval=1d"
            + $"&startTime={startTime}&endTime={endTime}&limit={PageSize}";
        var settings = options.Value;

        try
        {
            return await SendAsync(new Uri(settings.BaseUrl, path), cancellationToken);
        }
        catch (Exception exception) when (settings.FallbackBaseUrl is not null && IsServerSideFailure(exception, cancellationToken))
        {
            logger.LogWarning(exception, "Binance API at {BaseUrl} failed, retrying with {FallbackBaseUrl}",
                settings.BaseUrl, settings.FallbackBaseUrl);
            return await SendAsync(new Uri(settings.FallbackBaseUrl, path), cancellationToken);
        }
    }

    private async Task<List<Kline>> SendAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new BinanceApiException(
                $"Binance API returned {(int)response.StatusCode} for {uri.GetLeftPart(UriPartial.Path)}: {Truncate(body, 200)}",
                response.StatusCode,
                GetRetryAfter(response));
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseKlines(document.RootElement);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or IndexOutOfRangeException)
        {
            throw new BinanceApiException("Binance API returned klines in an unexpected format.", innerException: exception);
        }
    }

    /// <summary>
    /// Each candle is an array of 12 values. Used fields: 0 open time (ms), 1–4 open, high, low, close,
    /// 5 base asset volume, 6 close time (ms), 7 quote asset volume, 8 number of trades.
    /// Prices and volumes are strings, times are numbers.
    /// </summary>
    private static List<Kline> ParseKlines(JsonElement root)
    {
        var klines = new List<Kline>(root.GetArrayLength());

        foreach (var candle in root.EnumerateArray())
        {
            var openTime = DateTimeOffset.FromUnixTimeMilliseconds(candle[0].GetInt64());
            klines.Add(new Kline(
                Date: DateOnly.FromDateTime(openTime.UtcDateTime),
                OpenTime: openTime,
                CloseTime: DateTimeOffset.FromUnixTimeMilliseconds(candle[6].GetInt64()),
                Open: ParseDecimal(candle[1]),
                High: ParseDecimal(candle[2]),
                Low: ParseDecimal(candle[3]),
                Close: ParseDecimal(candle[4]),
                Volume: ParseDecimal(candle[5]),
                QuoteVolume: ParseDecimal(candle[7]),
                TradeCount: candle[8].GetInt64()));
        }

        return klines;
    }

    // Binance uses a dot as the decimal separator, so parsing must not depend on the system culture.
    private static decimal ParseDecimal(JsonElement value) =>
        decimal.Parse(value.GetString() ?? throw new FormatException("Missing numeric value."), NumberStyles.Number, CultureInfo.InvariantCulture);

    private static long ToUnixMilliseconds(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds();

    /// <summary>Network errors, timeouts and 5xx responses; 4xx responses are not retried on the fallback server.</summary>
    private static bool IsServerSideFailure(Exception exception, CancellationToken cancellationToken) => exception switch
    {
        HttpRequestException => true,
        TaskCanceledException => !cancellationToken.IsCancellationRequested,
        BinanceApiException { StatusCode: { } statusCode } => (int)statusCode >= 500,
        _ => false,
    };

    private TimeSpan? GetRetryAfter(HttpResponseMessage response) => response.Headers.RetryAfter switch
    {
        { Delta: { } delta } => delta,
        { Date: { } date } => date - timeProvider.GetUtcNow(),
        _ => null,
    };

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "…";
}
