namespace CryptoMarketAnalytics.Api.Services;

/// <summary>No data of the symbol has been downloaded from Binance yet, so there is nothing to serve.</summary>
public class DataNotAvailableException(string message) : Exception(message);
