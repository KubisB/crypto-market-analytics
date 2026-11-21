using CryptoMarketAnalytics.Api.Binance.Exceptions;
using CryptoMarketAnalytics.Api.Dtos;
using CryptoMarketAnalytics.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CryptoMarketAnalytics.Api.Controllers;

[ApiController]
[Route("api/candles")]
[Produces("application/json")]
public class CandlesController(IDailyCandleService candleService) : ControllerBase
{
    /// <summary>Daily candles of a trading pair.</summary>
    /// <remarks>
    /// Returns closed daily candles for the UTC days from <c>from</c> to <c>to</c>, inclusive, in ascending order.
    /// Today's candle is never returned, because it is not closed yet.
    /// </remarks>
    /// <param name="symbol">Trading pair: <c>BNBUSDT</c> or <c>BTCUSDT</c> (case-insensitive).</param>
    /// <param name="from">First UTC day, e.g. <c>2026-10-01</c>; not earlier than <c>Market:AnalysisStartDate</c> minus <c>Market:WarmupDays</c> from the configuration (2022-06-15 by default).</param>
    /// <param name="to">Last UTC day, inclusive, e.g. <c>2026-10-31</c>.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="200">Candles of the requested days; days without data are missing from the list.</response>
    /// <response code="400">Unsupported symbol, missing parameter or invalid date range.</response>
    /// <response code="502">Binance API is unavailable or returned an error.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DailyCandleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<DailyCandleDto>>> Get(
        [FromQuery, BindRequired] string symbol,
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        try
        {
            var candles = await candleService.GetAsync(symbol, from, to, cancellationToken);
            return Ok(candles.Select(DailyCandleDto.FromDomain).ToList());
        }
        catch (InvalidCandleRequestException exception)
        {
            return Problem(title: "Invalid request", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (BinanceApiException exception)
        {
            return Problem(title: "Binance API error", detail: exception.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
