namespace Arcanic.Result;

/// <summary>
/// Represents the type of error.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Represents a failure error.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Represents a validation error.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Represents a conflict error.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Represents a not found error.
    /// </summary>
    NotFound = 3
}