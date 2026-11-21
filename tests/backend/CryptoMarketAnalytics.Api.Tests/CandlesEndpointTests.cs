using System.Net;
using System.Net.Http.Json;
using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Dtos;
using CryptoMarketAnalytics.Api.Synchronization;
using CryptoMarketAnalytics.Api.Tests.Binance;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoMarketAnalytics.Api.Tests;

public class CandlesEndpointTests : IDisposable
{
    private static readonly DailyCandle SampleCandle = new(
        new DateOnly(2026, 10, 12), 754.24m, 758.03m, 732.26m, 732.91m, 108635.607m, 81335469.84276m);

    // Yesterday, the last closed day, is 2026-11-02.
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"crypto-market-analytics-tests-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program>? _factory;

    [Fact]
    public async Task Get_ReturnsCandlesAsDtos()
    {
        var client = await CreateSynchronisedClientAsync(new StubBinanceClient(() => [SampleCandle]));

        var response = await client.GetAsync("/api/candles?symbol=bnbusdt&from=2026-10-12&to=2026-10-12");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var range = await response.Content.ReadFromJsonAsync<CandleRangeDto>();
        Assert.Equal(DailyCandleDto.FromDomain(SampleCandle), Assert.Single(range!.Candles));
        Assert.Empty(range.MissingDays);
    }

    [Fact]
    public async Task Get_ServesRequestsFromRepository()
    {
        var binance = new StubBinanceClient(() => [SampleCandle]);
        var client = await CreateSynchronisedClientAsync(binance);
        var callsAfterSync = binance.Calls;

        await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-12&to=2026-10-12");
        await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-12&to=2026-10-12");

        Assert.Equal(callsAfterSync, binance.Calls);
    }

    [Theory]
    [InlineData("/api/candles?symbol=ETHUSDT&from=2026-10-01&to=2026-10-31")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2026-10-31&to=2026-10-01")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2022-06-14&to=2026-10-01")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2026-10-01&to=2026-11-03")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2026-10-01")]
    public async Task Get_RejectsInvalidRequest(string url)
    {
        var client = CreateClient(new StubBinanceClient(() => throw new InvalidOperationException("Must not be called.")));

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsServiceUnavailableWhenNothingWasDownloaded()
    {
        var client = await CreateSynchronisedClientAsync(new StubBinanceClient(() =>
            throw new BinanceApiException("Binance API returned 503", HttpStatusCode.ServiceUnavailable)));

        var response = await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-01&to=2026-10-31");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    public void Dispose()
    {
        _factory?.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }

    /// <summary>Client of the API after one synchronisation with the stub Binance.</summary>
    private async Task<HttpClient> CreateSynchronisedClientAsync(IBinanceClient binanceClient)
    {
        var client = CreateClient(binanceClient);
        await using var scope = _factory!.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DailyCandleSynchronizer>().SyncAllAsync();
        return client;
    }

    /// <summary>API with a temporary database, no background synchronisation, a fixed clock and a stub instead of Binance.</summary>
    private HttpClient CreateClient(IBinanceClient binanceClient)
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseSetting("Storage:DatabasePath", _databasePath)
            .UseSetting("Synchronization:Enabled", "false")
            .ConfigureTestServices(services =>
            {
                services.AddSingleton(binanceClient);
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            }));
        return _factory.CreateClient();
    }

    private class StubBinanceClient(Func<IReadOnlyList<DailyCandle>> getCandles) : IBinanceClient
    {
        public int Calls { get; private set; }

        public Task<IReadOnlyList<DailyCandle>> GetDailyCandlesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(getCandles());
        }

        public Task<string> GetSymbolStatusAsync(string symbol, CancellationToken cancellationToken = default) =>
            Task.FromResult("TRADING");
    }
}
