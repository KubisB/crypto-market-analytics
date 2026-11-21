using Microsoft.EntityFrameworkCore;

namespace CryptoMarketAnalytics.Api.Persistence;

/// <summary>
/// SQLite database with the downloaded candles. The schema is created and updated by EF Core migrations
/// (folder <c>Persistence/Migrations/</c>), applied when the application starts.
/// </summary>
public class MarketDbContext(DbContextOptions<MarketDbContext> options) : DbContext(options)
{
    /// <summary>Maximum stored length of <see cref="SyncStateRecord.LastError"/>.</summary>
    public const int ErrorMaxLength = 1000;

    public DbSet<CandleRecord> Candles => Set<CandleRecord>();

    public DbSet<SyncStateRecord> SyncStates => Set<SyncStateRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandleRecord>(candle =>
        {
            candle.ToTable("Candles");
            candle.HasKey(c => new { c.Symbol, c.Date });
            candle.Property(c => c.Symbol).HasMaxLength(20);
        });

        modelBuilder.Entity<SyncStateRecord>(state =>
        {
            state.ToTable("SyncStates");
            state.HasKey(s => s.Symbol);
            state.Property(s => s.Symbol).HasMaxLength(20);
            state.Property(s => s.ExchangeStatus).HasMaxLength(20);
            state.Property(s => s.LastError).HasMaxLength(ErrorMaxLength);
        });
    }
}
