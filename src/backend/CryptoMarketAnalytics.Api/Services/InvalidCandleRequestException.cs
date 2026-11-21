namespace CryptoMarketAnalytics.Api.Services;

/// <summary>The request for candles breaks a business rule: unsupported symbol or invalid date range.</summary>
public class InvalidCandleRequestException(string message) : Exception(message);
