using System.ComponentModel.DataAnnotations;

namespace CryptoMarketAnalytics.Api.Configuration;

/// <summary>Local storage of candles, section "Storage" in appsettings.json. Validated at startup.</summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Path of the SQLite database file. A relative path is resolved against the project directory
    /// (content root), so the default <c>../../../data-base/market.db</c> points to <c>data-base/</c> in the repository root.
    /// </summary>
    [Required]
    public string DatabasePath { get; set; } = "";

    /// <summary>Absolute path of the database file.</summary>
    public string GetFullDatabasePath(string contentRootPath) =>
        Path.GetFullPath(Path.Combine(contentRootPath, DatabasePath));
}
