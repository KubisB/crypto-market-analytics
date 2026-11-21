using CryptoMarketAnalytics.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMarketAnalytics.Api.Dtos;

/// <summary>Error response (RFC 9457 problem details) with a stable code for the frontend.</summary>
public class ApiProblemDetails : ProblemDetails
{
    /// <summary>
    /// Machine-readable error code, one of: <c>invalid-parameter</c>, <c>unsupported-symbol</c>, <c>invalid-date-range</c>,
    /// <c>date-before-earliest</c>, <c>date-not-closed</c>, <c>data-not-available</c>. The frontend shows the matching
    /// Polish message; <c>detail</c> is an English description for developers.
    /// </summary>
    public string Code { get; set; } = "";

    public static ApiProblemDetails Create(int status, string title, string code, string? detail = null) => new()
    {
        Status = status,
        Title = title,
        Code = code,
        Detail = detail,
    };

    /// <summary>Validation error of the request parameters (missing or unparseable), with the code <see cref="ErrorCodes.InvalidParameter"/>.</summary>
    public static ApiProblemDetails FromValidation(ValidationProblemDetails validation) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Invalid request parameters",
        Code = ErrorCodes.InvalidParameter,
        Detail = string.Join(" ", validation.Errors.SelectMany(error => error.Value)),
    };
}
