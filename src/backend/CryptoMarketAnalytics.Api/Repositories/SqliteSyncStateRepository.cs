using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CryptoMarketAnalytics.Api.Repositories;

public class SqliteSyncStateRepository(MarketDbContext db) : ISyncStateRepository
{
    public async Task<SymbolSyncState?> GetAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var record = await db.SyncStates.AsNoTracking().SingleOrDefaultAsync(s => s.Symbol == symbol, cancellationToken);
        return record is null
            ? null
            : new SymbolSyncState(record.Symbol, record.FirstDate, record.LastDate, record.LastSyncedAt,
                record.LastAttemptAt, record.LastError, record.ExchangeStatus);
    }

    public async Task SaveAsync(SymbolSyncState state, CancellationToken cancellationToken = default)
    {
        var record = await db.SyncStates.SingleOrDefaultAsync(s => s.Symbol == state.Symbol, cancellationToken);
        if (record is null)
        {
            record = new SyncStateRecord { Symbol = state.Symbol };
            db.SyncStates.Add(record);
        }

        record.FirstDate = state.FirstDate;
        record.LastDate = state.LastDate;
        record.LastSyncedAt = state.LastSyncedAt;
        record.LastAttemptAt = state.LastAttemptAt;
        record.LastError = state.LastError is { Length: > MarketDbContext.ErrorMaxLength } error
            ? error[..MarketDbContext.ErrorMaxLength]
            : state.LastError;
        record.ExchangeStatus = state.ExchangeStatus;

        await db.SaveChangesAsync(cancellationToken);
    }
}
