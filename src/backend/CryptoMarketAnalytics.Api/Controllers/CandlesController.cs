using CryptoMarketAnalytics.Api.Dtos;
using CryptoMarketAnalytics.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CryptoMarketAnalytics.Api.Controllers;

[ApiController]
[Route("api/candles")]
public class CandlesController(IDailyCandleService candleService) : ControllerBase
{
    /// <summary>Daily candles of a trading pair.</summary>
    /// <remarks>
    /// Returns the stored closed daily candles for the UTC days from <c>from</c> to <c>to</c>, inclusive, in ascending order,
    /// and the days of the range without a valid candle. Data is read from the local database, which the background
    /// synchronisation keeps up to date; <c>GET /api/data-status</c> tells how fresh it is.
    /// </remarks>
    /// <param name="symbol">Trading pair from <c>Market:Symbols</c>, e.g. <c>BNBUSDT</c> (case-insensitive).</param>
    /// <param name="from">First UTC day, e.g. <c>2026-10-01</c>; not earlier than <c>earliestDate</c> from <c>GET /api/data-status</c>.</param>
    /// <param name="to">Last UTC day, inclusive, e.g. <c>2026-10-31</c>; at most yesterday, because today's candle is not closed yet.</param>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="200">Candles of the requested days and the missing days.</response>
    /// <response code="400">
    /// Codes: <c>invalid-parameter</c> (missing or malformed parameter), <c>unsupported-symbol</c>, <c>invalid-date-range</c>
    /// (<c>from</c> after <c>to</c>), <c>date-before-earliest</c>, <c>date-not-closed</c> (<c>to</c> is today or later).
    /// </response>
    /// <response code="503">Code <c>data-not-available</c>: nothing has been downloaded from Binance yet; try again later.</response>
    [HttpGet]
    [ProducesResponseType<CandleRangeDto>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public async Task<ActionResult<CandleRangeDto>> Get(
        [FromQuery, BindRequired] string symbol,
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        try
        {
            var range = await candleService.GetAsync(symbol, from, to, cancellationToken);
            return Ok(CandleRangeDto.FromDomain(range));
        }
        catch (InvalidCandleRequestException exception)
        {
            return Problem(ApiProblemDetails.Create(StatusCodes.Status400BadRequest, "Invalid request", exception.Code, exception.Message));
        }
        catch (DataNotAvailableException exception)
        {
            return Problem(ApiProblemDetails.Create(StatusCodes.Status503ServiceUnavailable, "Data not available", ErrorCodes.DataNotAvailable, exception.Message));
        }
    }

    private ObjectResult Problem(ApiProblemDetails problem) => new(problem)
    {
        StatusCode = problem.Status,
        ContentTypes = { "application/problem+json" },
    };
}
