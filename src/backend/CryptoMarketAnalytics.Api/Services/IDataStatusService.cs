using CryptoMarketAnalytics.Api.Domain;

namespace CryptoMarketAnalytics.Api.Services;

public interface IDataStatusService
{
    /// <summary>What data the application has for every configured symbol and how fresh it is.</summary>
    Task<DataStatus> GetAsync(CancellationToken cancellationToken = default);
}
