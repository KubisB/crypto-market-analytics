using System.Net;
using System.Net.Http.Json;
using CryptoMarketAnalytics.Api.Binance;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoMarketAnalytics.Api.Tests;

public class MarketDataEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Kline SampleKline = new(
        new DateOnly(2026, 10, 12),
        DateTimeOffset.FromUnixTimeMilliseconds(1791763200000),
        DateTimeOffset.FromUnixTimeMilliseconds(1791849599999),
        754.24m, 758.03m, 732.26m, 732.91m, 108635.607m, 81335469.84276m, 509024);

    [Fact]
    public async Task GetKlines_ReturnsCandlesFromBinanceClient()
    {
        var client = CreateClient(new StubBinanceClient(_ => [SampleKline]));

        var response = await client.GetAsync("/api/market/klines?symbol=bnbusdt&from=2026-10-12&to=2026-10-12");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var klines = await response.Content.ReadFromJsonAsync<List<Kline>>();
        Assert.Equal(SampleKline, Assert.Single(klines!));
    }

    [Theory]
    [InlineData("/api/market/klines?symbol=ETHUSDT&from=2026-10-01&to=2026-10-31")]
    [InlineData("/api/market/klines?symbol=BNBUSDT&from=2026-10-31&to=2026-10-01")]
    [InlineData("/api/market/klines?symbol=BNBUSDT&from=2016-12-31&to=2026-10-01")]
    [InlineData("/api/market/klines?symbol=BNBUSDT&from=2026-10-01")]
    public async Task GetKlines_RejectsInvalidRequest(string url)
    {
        var client = CreateClient(new StubBinanceClient(_ => throw new InvalidOperationException("Must not be called.")));

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetKlines_ReturnsBadGatewayWhenBinanceFails()
    {
        var client = CreateClient(new StubBinanceClient(_ =>
            throw new BinanceApiException("Binance API returned 503", HttpStatusCode.ServiceUnavailable)));

        var response = await client.GetAsync("/api/market/klines?symbol=BNBUSDT&from=2026-10-01&to=2026-10-31");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    private HttpClient CreateClient(IBinanceClient binanceClient) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton(binanceClient))).CreateClient();

    private sealed class StubBinanceClient(Func<string, IReadOnlyList<Kline>> getKlines) : IBinanceClient
    {
        public Task<IReadOnlyList<Kline>> GetDailyKlinesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
            Task.FromResult(getKlines(symbol));
    }
}
