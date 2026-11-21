using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Repositories;

/// <summary>Stores what has been downloaded from Binance for each symbol.</summary>
public interface ISyncStateRepository
{
    /// <summary>State of <paramref name="symbol"/>, or null if it has never been synchronised.</summary>
    Task<SymbolSyncState?> GetAsync(string symbol, CancellationToken cancellationToken = default);

    Task SaveAsync(SymbolSyncState state, CancellationToken cancellationToken = default);
}
