using CryptoMarketAnalytics.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Persistence;

public static class DatabaseMigration
{
    /// <summary>Creates the database folder if needed and applies pending migrations, so the application starts with an up-to-date schema.</summary>
    public static void MigrateDatabase(this WebApplication app)
    {
        var storage = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
        Directory.CreateDirectory(Path.GetDirectoryName(storage.GetFullDatabasePath(app.Environment.ContentRootPath))!);

        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<MarketDbContext>().Database.Migrate();
    }
}
