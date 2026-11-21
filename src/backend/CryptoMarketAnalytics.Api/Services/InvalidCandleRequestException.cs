namespace CryptoMarketAnalytics.Api.Services;

/// <summary>The request for candles breaks a business rule: unsupported symbol or invalid date range.</summary>
/// <param name="code">One of <see cref="ErrorCodes"/>.</param>
/// <param name="message">Description for developers, in English.</param>
public class InvalidCandleRequestException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
