using System.Text.Json;
using CryptoMarketAnalytics.Api.Binance.Configuration;
using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Binance.Mapping;
using CryptoMarketAnalytics.Api.Domain;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Binance.Client;

/// <summary>
/// Client of the public Binance REST API (<c>GET /api/v3/klines</c>, <c>GET /api/v3/exchangeInfo</c>),
/// as described in chapter 10 of docs/requirements/wymagania-i-zakres.md. No account or API key is needed.
/// </summary>
public class BinanceClient(
    HttpClient httpClient,
    IOptions<BinanceOptions> options,
    TimeProvider timeProvider,
    ILogger<BinanceClient> logger) : IBinanceClient
{
    /// <summary>Maximum number of klines Binance returns per request.</summary>
    public const int PageSize = 1000;

    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    public async Task<IReadOnlyList<DailyCandle>> GetDailyCandlesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        if (from > to)
        {
            throw new ArgumentException("'from' must not be later than 'to'.", nameof(from));
        }

        var startTime = StartOfDay(from);
        var endTime = StartOfDay(to.AddDays(1)).AddMilliseconds(-1);
        var now = timeProvider.GetUtcNow();
        var candles = new List<DailyCandle>();

        // Binance returns at most 1000 klines per request; the next page starts one day after
        // the open time of the last kline received, until a page is shorter than the limit.
        while (startTime <= endTime)
        {
            var path = $"api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval=1d"
                + $"&startTime={startTime.ToUnixTimeMilliseconds()}&endTime={endTime.ToUnixTimeMilliseconds()}&limit={PageSize}";
            var page = await GetAsync(path, BinanceKlineMapper.FromJson, cancellationToken);

            foreach (var reason in page.Rejected)
            {
                logger.LogWarning("Rejected {Symbol} kline from Binance: {Reason}", symbol, reason);
            }

            // The API also returns the current, unfinished day; only closed klines are kept.
            candles.AddRange(page.Klines.Where(kline => kline.CloseTime <= now).Select(BinanceKlineMapper.ToDailyCandle));

            if (page.RowCount < PageSize || page.LastOpenTime is null)
            {
                break;
            }

            startTime = page.LastOpenTime.Value + OneDay;
        }

        return candles;
    }

    public Task<string> GetSymbolStatusAsync(string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        return GetAsync(
            $"api/v3/exchangeInfo?symbol={Uri.EscapeDataString(symbol)}",
            root => BinanceKlineMapper.StatusFromExchangeInfo(root, symbol),
            cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, Func<JsonElement, T> read, CancellationToken cancellationToken)
    {
        var settings = options.Value;

        try
        {
            try
            {
                return await SendAsync(new Uri(settings.BaseUrl!, path), read, cancellationToken);
            }
            catch (Exception exception) when (settings.FallbackBaseUrl is not null && IsServerSideFailure(exception, cancellationToken))
            {
                logger.LogWarning(exception, "Binance API at {BaseUrl} failed, retrying with {FallbackBaseUrl}",
                    settings.BaseUrl, settings.FallbackBaseUrl);
                return await SendAsync(new Uri(settings.FallbackBaseUrl, path), read, cancellationToken);
            }
        }
        catch (Exception exception) when (IsConnectionFailure(exception, cancellationToken))
        {
            // Callers handle every Binance failure as BinanceApiException; cancellation by the caller is not wrapped.
            throw new BinanceApiException(
                $"Binance API could not be reached for {path.Split('?')[0]}: {exception.Message}", innerException: exception);
        }
    }

    private async Task<T> SendAsync<T>(Uri uri, Func<JsonElement, T> read, CancellationToken cancellationToken)
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

        if (response.Headers.TryGetValues("X-MBX-USED-WEIGHT-1M", out var usedWeight))
        {
            logger.LogDebug("Binance request weight used in the last minute: {UsedWeight}", string.Join(",", usedWeight));
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        try
        {
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return read(document.RootElement);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new BinanceApiException($"Binance API returned an unexpected response for {uri.GetLeftPart(UriPartial.Path)}.", innerException: exception);
        }
    }

    private static DateTimeOffset StartOfDay(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    /// <summary>Network errors, timeouts and 5xx responses; 4xx responses are not retried on the fallback server.</summary>
    private static bool IsServerSideFailure(Exception exception, CancellationToken cancellationToken) => exception switch
    {
        BinanceApiException { StatusCode: { } statusCode } => (int)statusCode >= 500,
        _ => IsConnectionFailure(exception, cancellationToken),
    };

    /// <summary>
    /// The server could not be reached or the connection broke while reading the response. A timeout of the
    /// <see cref="HttpClient"/> is a <see cref="TaskCanceledException"/> without cancellation by the caller.
    /// </summary>
    private static bool IsConnectionFailure(Exception exception, CancellationToken cancellationToken) => exception switch
    {
        HttpRequestException or IOException => true,
        TaskCanceledException => !cancellationToken.IsCancellationRequested,
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
