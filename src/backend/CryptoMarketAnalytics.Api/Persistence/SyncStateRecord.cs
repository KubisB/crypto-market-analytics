namespace CryptoMarketAnalytics.Api.Persistence;

/// <summary>Row of the <c>SyncStates</c> table: what has been downloaded for one symbol.</summary>
public class SyncStateRecord
{
    public string Symbol { get; set; } = "";
    public DateOnly? FirstDate { get; set; }
    public DateOnly? LastDate { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
    public string? ExchangeStatus { get; set; }
}
