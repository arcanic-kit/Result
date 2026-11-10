using Arcanic.Result;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Extensions;

/// <summary>
/// The single place where <see cref="ErrorType"/> is translated into HTTP.
/// </summary>
public static class ErrorHttpExtensions
{
    /// <summary>
    /// Maps an <see cref="ErrorType"/> to its HTTP status code.
    /// </summary>
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Converts an error into an RFC 9457 problem details response, carrying the error code
    /// as a <c>code</c> extension so clients can branch on it.
    /// </summary>
    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        var result = controller.Problem(
            statusCode: error.Type.ToStatusCode(),
            title: error.Type.ToString(),
            detail: error.Description);

        if (result.Value is ProblemDetails problem)
        {
            problem.Extensions["code"] = error.Code;
        }

        return result;
    }
}
