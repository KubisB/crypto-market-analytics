using CryptoMarketAnalytics.Api.Domain;
using CryptoMarketAnalytics.Api.Validation;
using Microsoft.Extensions.Logging.Abstractions;

namespace CryptoMarketAnalytics.Api.Tests.Validation;

public class DailyCandleValidatorTests
{
    private static readonly DailyCandle Valid = new(new DateOnly(2026, 10, 12), 754.24m, 758.03m, 732.26m, 732.91m, 108635.607m, 81335469.84276m);

    private readonly DailyCandleValidator _validator = new(NullLogger<DailyCandleValidator>.Instance);

    [Fact]
    public void ValidCandle_IsAccepted()
    {
        Assert.Null(DailyCandleValidator.GetRejectionReason(Valid));
    }

    [Fact]
    public void ZeroVolume_IsAccepted()
    {
        Assert.Null(DailyCandleValidator.GetRejectionReason(Valid with { Volume = 0, QuoteVolume = 0 }));
    }

    public static TheoryData<DailyCandle> InvalidCandles => new()
    {
        Valid with { Open = 0 },
        Valid with { Close = -1 },
        Valid with { Low = 0 },
        Valid with { Volume = -1 },
        Valid with { QuoteVolume = -0.01m },
        Valid with { Low = 740m },   // above close 732.91
        Valid with { High = 750m },  // below open 754.24
    };

    [Theory]
    [MemberData(nameof(InvalidCandles))]
    public void InvalidCandle_IsRejected(DailyCandle candle)
    {
        Assert.NotNull(DailyCandleValidator.GetRejectionReason(candle));
        Assert.Empty(_validator.Validate("BNBUSDT", [candle]));
    }

    [Fact]
    public void Validate_SortsByDate()
    {
        var next = Valid with { Date = Valid.Date.AddDays(1) };

        Assert.Equal([Valid, next], _validator.Validate("BNBUSDT", [next, Valid]));
    }

    [Fact]
    public void Validate_RejectsDayWithDifferentDuplicates()
    {
        var other = Valid with { Close = 740m };
        var next = Valid with { Date = Valid.Date.AddDays(1) };

        Assert.Equal([next], _validator.Validate("BNBUSDT", [Valid, other, next]));
    }

    [Fact]
    public void Validate_KeepsExactCopyOnce()
    {
        Assert.Equal([Valid], _validator.Validate("BNBUSDT", [Valid, Valid with { }]));
    }
}
