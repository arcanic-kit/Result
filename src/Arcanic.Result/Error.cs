namespace Arcanic.Result;

/// <summary>
/// Represents an error with a code, description, and type.
/// </summary>
/// <param name="Code">A short identifier for the error (e.g. <c>Product.NotFound</c>).</param>
/// <param name="Description">A human-readable message describing the error.</param>
/// <param name="Type">The category of the error.</param>
[DebuggerDisplay("{Type}: {Code} - {Description}")]
public sealed record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>
    /// Gets a sentinel error instance used to represent the absence of an error on a successful result.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Creates a new failure error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with failure type.</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a new validation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with validation type.</returns>
    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    /// <summary>
    /// Creates a new conflict error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with conflict type.</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a new not found error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new Error instance with not found type.</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
}