using CryptoMarketAnalytics.Api.Binance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CryptoMarketAnalytics.Api.Controllers;

[ApiController]
[Route("api/market")]
public sealed class MarketDataController(IBinanceClient binanceClient) : ControllerBase
{
    // Only the pairs analysed in the project, so the endpoint cannot be used as an open proxy to Binance.
    private static readonly string[] SupportedSymbols = ["BNBUSDT", "BTCUSDT"];

    // Both pairs are listed on Binance since 2017.
    private static readonly DateOnly EarliestDate = new(2017, 1, 1);

    /// <summary>Closed daily candles fetched directly from Binance for the UTC days from <paramref name="from"/> to <paramref name="to"/>.</summary>
    [HttpGet("klines")]
    [ProducesResponseType<IReadOnlyList<Kline>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<Kline>>> GetKlines(
        [FromQuery, BindRequired] string symbol,
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        var normalizedSymbol = symbol.ToUpperInvariant();
        if (!SupportedSymbols.Contains(normalizedSymbol))
        {
            return Problem(
                title: "Unsupported symbol",
                detail: $"Supported symbols: {string.Join(", ", SupportedSymbols)}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (from > to || from < EarliestDate)
        {
            return Problem(
                title: "Invalid date range",
                detail: $"'from' must not be later than 'to' and not earlier than {EarliestDate:yyyy-MM-dd}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            return Ok(await binanceClient.GetDailyKlinesAsync(normalizedSymbol, from, to, cancellationToken));
        }
        catch (BinanceApiException exception)
        {
            return Problem(
                title: "Binance API error",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
