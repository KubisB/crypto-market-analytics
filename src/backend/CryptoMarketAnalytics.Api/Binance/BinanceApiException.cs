using System.Net;

namespace CryptoMarketAnalytics.Api.Binance;

/// <summary>The Binance API rejected a request or returned an unexpected response.</summary>
public sealed class BinanceApiException(string message, HttpStatusCode? statusCode = null, TimeSpan? retryAfter = null, Exception? innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;

    /// <summary>Wait time from the Retry-After header, sent with 429 (rate limit) and 418 (IP ban).</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;

    public bool IsRateLimited => StatusCode is HttpStatusCode.TooManyRequests || (int?)StatusCode == 418;
}
