using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CryptoMarketAnalytics.Api.Repositories;

public class SqliteDailyCandleRepository(MarketDbContext db) : IDailyCandleRepository
{
    public async Task<IReadOnlyList<DailyCandle>> GetAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await db.Candles
            .AsNoTracking()
            .Where(c => c.Symbol == symbol && c.Date >= from && c.Date <= to)
            .OrderBy(c => c.Date)
            .Select(c => new DailyCandle(c.Date, c.Open, c.High, c.Low, c.Close, c.Volume, c.QuoteVolume))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(string symbol, IReadOnlyList<DailyCandle> candles, CancellationToken cancellationToken = default)
    {
        if (candles.Count == 0)
        {
            return;
        }

        var from = candles.Min(c => c.Date);
        var to = candles.Max(c => c.Date);
        var stored = await db.Candles
            .Where(c => c.Symbol == symbol && c.Date >= from && c.Date <= to)
            .ToDictionaryAsync(c => c.Date, cancellationToken);

        foreach (var candle in candles)
        {
            if (!stored.TryGetValue(candle.Date, out var record))
            {
                record = new CandleRecord { Symbol = symbol, Date = candle.Date };
                db.Candles.Add(record);
                stored[candle.Date] = record;
            }

            record.Open = candle.Open;
            record.High = candle.High;
            record.Low = candle.Low;
            record.Close = candle.Close;
            record.Volume = candle.Volume;
            record.QuoteVolume = candle.QuoteVolume;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
