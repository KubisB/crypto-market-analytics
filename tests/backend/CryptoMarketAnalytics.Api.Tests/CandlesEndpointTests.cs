using System.Net;
using System.Net.Http.Json;
using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoMarketAnalytics.Api.Tests;

public class CandlesEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DailyCandle SampleCandle = new(
        new DateOnly(2026, 10, 12), 754.24m, 758.03m, 732.26m, 732.91m, 108635.607m, 81335469.84276m);

    [Fact]
    public async Task Get_ReturnsCandlesAsDtos()
    {
        var client = CreateClient(new StubBinanceClient(() => [SampleCandle]));

        var response = await client.GetAsync("/api/candles?symbol=bnbusdt&from=2026-10-12&to=2026-10-12");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var candles = await response.Content.ReadFromJsonAsync<List<DailyCandleDto>>();
        Assert.Equal(DailyCandleDto.FromDomain(SampleCandle), Assert.Single(candles!));
    }

    [Fact]
    public async Task Get_ServesRepeatedRequestFromRepository()
    {
        var binance = new StubBinanceClient(() => [SampleCandle]);
        var client = CreateClient(binance);

        await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-12&to=2026-10-12");
        await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-12&to=2026-10-12");

        Assert.Equal(1, binance.Calls);
    }

    [Theory]
    [InlineData("/api/candles?symbol=ETHUSDT&from=2026-10-01&to=2026-10-31")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2026-10-31&to=2026-10-01")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2022-06-14&to=2026-10-01")]
    [InlineData("/api/candles?symbol=BNBUSDT&from=2026-10-01")]
    public async Task Get_RejectsInvalidRequest(string url)
    {
        var client = CreateClient(new StubBinanceClient(() => throw new InvalidOperationException("Must not be called.")));

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsBadGatewayWhenBinanceFails()
    {
        var client = CreateClient(new StubBinanceClient(() =>
            throw new BinanceApiException("Binance API returned 503", HttpStatusCode.ServiceUnavailable)));

        var response = await client.GetAsync("/api/candles?symbol=BNBUSDT&from=2026-10-01&to=2026-10-31");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    private HttpClient CreateClient(IBinanceClient binanceClient) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton(binanceClient))).CreateClient();

    private class StubBinanceClient(Func<IReadOnlyList<DailyCandle>> getCandles) : IBinanceClient
    {
        public int Calls { get; private set; }

        public Task<IReadOnlyList<DailyCandle>> GetDailyCandlesAsync(string symbol, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(getCandles());
        }
    }
}
