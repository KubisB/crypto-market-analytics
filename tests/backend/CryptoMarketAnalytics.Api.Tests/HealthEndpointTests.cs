using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace CryptoMarketAnalytics.Api.Tests;

public class HealthEndpointTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"crypto-market-analytics-tests-{Guid.NewGuid():N}.db");
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests()
    {
        // Temporary database and no background synchronisation, so the test neither calls Binance nor touches data-base/.
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseSetting("Storage:DatabasePath", _databasePath)
            .UseSetting("Synchronization:Enabled", "false"));
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    public void Dispose()
    {
        _factory.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
