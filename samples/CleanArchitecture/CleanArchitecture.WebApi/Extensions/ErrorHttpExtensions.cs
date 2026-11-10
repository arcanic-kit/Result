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
    /// Converts a failed result into an RFC 9457 problem details response. The status comes from
    /// the first error; every error is listed in an <c>errors</c> extension so clients see all of them.
    /// </summary>
    /// <remarks>
    /// Takes the whole result rather than an <see cref="Error"/>, because <c>Match</c>'s
    /// <c>onFailure</c> only receives the first error.
    /// </remarks>
    public static IActionResult ToProblem(this ControllerBase controller, Result failure)
    {
        if (failure.IsSuccess)
        {
            throw new ArgumentException("Cannot create a problem response from a successful result.", nameof(failure));
        }

        var first = failure.Error;

        var result = controller.Problem(
            statusCode: first.Type.ToStatusCode(),
            title: first.Type.ToString(),
            detail: failure.Errors.Count == 1 ? first.Description : "One or more errors occurred.");

        if (result.Value is ProblemDetails problem)
        {
            problem.Extensions["code"] = first.Code;
            problem.Extensions["errors"] = failure.Errors
                .Select(error => new { error.Code, error.Description })
                .ToArray();
        }

        return result;
    }
}
