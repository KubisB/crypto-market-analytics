using System.Globalization;
using System.Net;
using System.Web;
using CryptoMarketAnalytics.Api.Binance;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CryptoMarketAnalytics.Api.Tests.Binance;

public class BinanceClientTests
{
    private const long DayMilliseconds = 86_400_000;

    // Example candle from chapter 10.2 of docs/requirements/wymagania-i-zakres.md (BNBUSDT, 12.10.2026).
    private const string ExampleCandle = """
        [1791763200000, "754.24000000", "758.03000000", "732.26000000", "732.91000000",
         "108635.60700000", 1791849599999, "81335469.84276000", 509024,
         "45460.00700000", "34047834.79001000", "0"]
        """;

    private static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetDailyKlines_ParsesCandleFields()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json($"[{ExampleCandle}]"));

        var klines = await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

        var kline = Assert.Single(klines);
        Assert.Equal(new DateOnly(2026, 10, 12), kline.Date);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1791763200000), kline.OpenTime);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1791849599999), kline.CloseTime);
        Assert.Equal(754.24m, kline.Open);
        Assert.Equal(758.03m, kline.High);
        Assert.Equal(732.26m, kline.Low);
        Assert.Equal(732.91m, kline.Close);
        Assert.Equal(108635.607m, kline.Volume);
        Assert.Equal(81335469.84276m, kline.QuoteVolume);
        Assert.Equal(509024, kline.TradeCount);
    }

    [Fact]
    public async Task GetDailyKlines_ParsesNumbersIndependentlyOfCulture()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json($"[{ExampleCandle}]"));
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL"); // decimal comma

        try
        {
            var klines = await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

            Assert.Equal(754.24m, Assert.Single(klines).Open);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task GetDailyKlines_SendsExpectedQuery()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("[]"));

        await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2023, 1, 1), new(2023, 1, 31));

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://data-api.binance.vision/api/v3/klines", request.GetLeftPart(UriPartial.Path));
        var query = HttpUtility.ParseQueryString(request.Query);
        Assert.Equal("BNBUSDT", query["symbol"]);
        Assert.Equal("1d", query["interval"]);
        Assert.Equal(ToUnixMilliseconds(new(2023, 1, 1)).ToString(), query["startTime"]);
        Assert.Equal((ToUnixMilliseconds(new(2023, 2, 1)) - 1).ToString(), query["endTime"]);
        Assert.Equal("1000", query["limit"]);
    }

    [Fact]
    public async Task GetDailyKlines_RequestsNextPageWhenPageIsFull()
    {
        var firstDay = new DateOnly(2022, 6, 1);
        var handler = new FakeHttpMessageHandler(request =>
        {
            var startTime = long.Parse(HttpUtility.ParseQueryString(request.RequestUri!.Query)["startTime"]!);
            var count = startTime == ToUnixMilliseconds(firstDay) ? BinanceClient.PageSize : 3;
            return FakeHttpMessageHandler.Json(Candles(startTime, count));
        });

        var klines = await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", firstDay, new(2026, 1, 1));

        Assert.Equal(2, handler.Requests.Count);
        var secondStartTime = HttpUtility.ParseQueryString(handler.Requests[1].Query)["startTime"];
        var lastOpenTimeOfFirstPage = ToUnixMilliseconds(firstDay) + (BinanceClient.PageSize - 1) * DayMilliseconds;
        Assert.Equal((lastOpenTimeOfFirstPage + DayMilliseconds).ToString(), secondStartTime);
        Assert.Equal(BinanceClient.PageSize + 3, klines.Count);
        Assert.Equal(klines.Count, klines.Select(kline => kline.Date).Distinct().Count());
    }

    [Fact]
    public async Task GetDailyKlines_DropsUnclosedCandleOfCurrentDay()
    {
        var yesterday = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-1);
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(Candles(ToUnixMilliseconds(yesterday), 2)));

        var klines = await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", yesterday, yesterday.AddDays(1));

        Assert.Equal(yesterday, Assert.Single(klines).Date);
    }

    [Fact]
    public async Task GetDailyKlines_UsesFallbackServerOnServerError()
    {
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.Host == "data-api.binance.vision"
            ? FakeHttpMessageHandler.Json("{}", HttpStatusCode.ServiceUnavailable)
            : FakeHttpMessageHandler.Json($"[{ExampleCandle}]"));

        var klines = await CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

        Assert.Single(klines);
        Assert.Equal(["data-api.binance.vision", "api.binance.com"], handler.Requests.Select(uri => uri.Host));
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData((HttpStatusCode)418)]
    public async Task GetDailyKlines_ReportsRateLimitWithoutUsingFallback(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(_ =>
        {
            var response = FakeHttpMessageHandler.Json("""{"code":-1003,"msg":"Too many requests"}""", statusCode);
            response.Headers.RetryAfter = new(TimeSpan.FromSeconds(60));
            return response;
        });

        var exception = await Assert.ThrowsAsync<BinanceApiException>(() =>
            CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12)));

        Assert.True(exception.IsRateLimited);
        Assert.Equal(TimeSpan.FromSeconds(60), exception.RetryAfter);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetDailyKlines_ThrowsOnUnexpectedResponseFormat()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""[["not", "a", "candle"]]"""));

        await Assert.ThrowsAsync<BinanceApiException>(() =>
            CreateClient(handler).GetDailyKlinesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12)));
    }

    private static BinanceClient CreateClient(FakeHttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(new BinanceOptions()), new FixedTimeProvider(Now), NullLogger<BinanceClient>.Instance);

    private static long ToUnixMilliseconds(DateOnly date) =>
        new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds();

    /// <summary>JSON array of <paramref name="count"/> consecutive daily candles starting at <paramref name="firstOpenTime"/>.</summary>
    private static string Candles(long firstOpenTime, int count) =>
        "[" + string.Join(",", Enumerable.Range(0, count).Select(i =>
        {
            var openTime = firstOpenTime + i * DayMilliseconds;
            var closeTime = openTime + DayMilliseconds - 1;
            return $"""[{openTime},"1.0","2.0","0.5","1.5","10.0",{closeTime},"15.0",100,"5.0","7.5","0"]""";
        })) + "]";
}
