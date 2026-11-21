using CryptoMarketAnalytics.Api.Configuration;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Repositories;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Services;

public class DataStatusService(
    ISyncStateRepository states,
    IOptions<MarketOptions> options,
    TimeProvider timeProvider) : IDataStatusService
{
    public async Task<DataStatus> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var symbols = new List<SymbolSyncState>();
        foreach (var symbol in settings.Symbols.Select(symbol => symbol.Trim().ToUpperInvariant()))
        {
            symbols.Add(await states.GetAsync(symbol, cancellationToken) ?? new SymbolSyncState(symbol));
        }

        var yesterday = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).AddDays(-1);
        return new DataStatus(yesterday, settings.AnalysisStartDate, settings.EarliestDate, symbols);
    }
}
