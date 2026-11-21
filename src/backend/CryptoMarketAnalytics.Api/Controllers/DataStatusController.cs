using CryptoMarketAnalytics.Api.Dtos;
using CryptoMarketAnalytics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMarketAnalytics.Api.Controllers;

[ApiController]
[Route("api/data-status")]
[Produces("application/json")]
public class DataStatusController(IDataStatusService dataStatusService) : ControllerBase
{
    /// <summary>What data the application has and how fresh it is.</summary>
    /// <remarks>
    /// Used by the footer ("Dane do dnia {data}, ostatnie pobranie {data} o {godzina} UTC"), the period shortcuts
    /// (counted back from <c>lastClosedDate</c>) and the message shown when Binance could not be reached
    /// (<c>isUpToDate</c> is false). The response is always 200, also before the first download.
    /// </remarks>
    /// <param name="cancellationToken">Cancelled when the client disconnects.</param>
    /// <response code="200">State of the data.</response>
    [HttpGet]
    [ProducesResponseType<DataStatusDto>(StatusCodes.Status200OK)]
    public async Task<DataStatusDto> Get(CancellationToken cancellationToken) =>
        DataStatusDto.FromDomain(await dataStatusService.GetAsync(cancellationToken));
}
