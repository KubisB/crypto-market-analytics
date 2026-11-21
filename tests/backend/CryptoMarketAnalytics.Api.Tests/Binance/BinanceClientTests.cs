using System.Globalization;
using System.Net;
using System.Web;
using CryptoMarketAnalytics.Api.Binance.Client;
using CryptoMarketAnalytics.Api.Binance.Configuration;
using CryptoMarketAnalytics.Api.Binance.Exceptions;
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

        var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

        var kline = Assert.Single(klines);
        Assert.Equal(new DateOnly(2026, 10, 12), kline.Date);
        Assert.Equal(754.24m, kline.Open);
        Assert.Equal(758.03m, kline.High);
        Assert.Equal(732.26m, kline.Low);
        Assert.Equal(732.91m, kline.Close);
        Assert.Equal(108635.607m, kline.Volume);
        Assert.Equal(81335469.84276m, kline.QuoteVolume);
    }

    [Fact]
    public async Task GetDailyKlines_ParsesNumbersIndependentlyOfCulture()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json($"[{ExampleCandle}]"));
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL"); // decimal comma

        try
        {
            var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

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

        await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2023, 1, 1), new(2023, 1, 31));

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

        var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", firstDay, new(2026, 1, 1));

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

        var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", yesterday, yesterday.AddDays(1));

        Assert.Equal(yesterday, Assert.Single(klines).Date);
    }

    [Fact]
    public async Task GetDailyKlines_UsesFallbackServerOnServerError()
    {
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.Host == "data-api.binance.vision"
            ? FakeHttpMessageHandler.Json("{}", HttpStatusCode.ServiceUnavailable)
            : FakeHttpMessageHandler.Json($"[{ExampleCandle}]"));

        var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12));

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
            CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12)));

        Assert.True(exception.IsRateLimited);
        Assert.Equal(TimeSpan.FromSeconds(60), exception.RetryAfter);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task GetDailyKlines_ThrowsWhenResponseIsNotAnArray()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"code":0}"""));

        await Assert.ThrowsAsync<BinanceApiException>(() =>
            CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 12)));
    }

    [Theory]
    [InlineData("""["not", "a", "candle"]""")]
    // Price that is not a number.
    [InlineData("""[1791763200000, "NaN", "758.03", "732.26", "732.91", "108635.607", 1791849599999, "81335469.84", 509024, "0", "0", "0"]""")]
    // Open time not at 00:00 UTC.
    [InlineData("""[1791766800000, "754.24", "758.03", "732.26", "732.91", "108635.607", 1791853199999, "81335469.84", 509024, "0", "0", "0"]""")]
    // Lasts one hour instead of one day.
    [InlineData("""[1791763200000, "754.24", "758.03", "732.26", "732.91", "108635.607", 1791766799999, "81335469.84", 509024, "0", "0", "0"]""")]
    public async Task GetDailyKlines_SkipsInvalidKlineAndKeepsTheRest(string invalidKline)
    {
        var nextDay = """[1791849600000, "1.0", "2.0", "0.5", "1.5", "10.0", 1791935999999, "15.0", 100, "0", "0", "0"]""";
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json($"[{invalidKline},{nextDay}]"));

        var klines = await CreateClient(handler).GetDailyCandlesAsync("BNBUSDT", new(2026, 10, 12), new(2026, 10, 13));

        Assert.Equal(new DateOnly(2026, 10, 13), Assert.Single(klines).Date);
    }

    [Fact]
    public async Task GetSymbolStatus_ReadsStatusFromExchangeInfo()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json(
            """{"timezone":"UTC","symbols":[{"symbol":"BNBUSDT","status":"TRADING","baseAsset":"BNB"}]}"""));

        var status = await CreateClient(handler).GetSymbolStatusAsync("BNBUSDT");

        Assert.Equal("TRADING", status);
        Assert.Equal("https://data-api.binance.vision/api/v3/exchangeInfo?symbol=BNBUSDT", Assert.Single(handler.Requests).ToString());
    }

    [Fact]
    public async Task GetSymbolStatus_ThrowsWhenSymbolIsMissing()
    {
        var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"symbols":[]}"""));

        await Assert.ThrowsAsync<BinanceApiException>(() => CreateClient(handler).GetSymbolStatusAsync("BNBUSDT"));
    }

    private static readonly BinanceOptions TestOptions = new()
    {
        BaseUrl = new Uri("https://data-api.binance.vision"),
        FallbackBaseUrl = new Uri("https://api.binance.com"),
        Timeout = TimeSpan.FromSeconds(30),
        UserAgent = "CryptoMarketAnalytics.Tests",
    };

    private static BinanceClient CreateClient(FakeHttpMessageHandler handler) =>
        new(new HttpClient(handler), Options.Create(TestOptions), new FixedTimeProvider(Now), NullLogger<BinanceClient>.Instance);

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
