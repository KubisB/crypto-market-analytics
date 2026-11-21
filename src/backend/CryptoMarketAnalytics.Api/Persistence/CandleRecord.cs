namespace CryptoMarketAnalytics.Api.Persistence;

/// <summary>Row of the <c>Candles</c> table: one daily candle of one symbol.</summary>
public class CandleRecord
{
    public string Symbol { get; set; } = "";
    public DateOnly Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal QuoteVolume { get; set; }
}
