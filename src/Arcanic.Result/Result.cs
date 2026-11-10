using System.Collections.ObjectModel;

namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <remarks>
/// This is the base of every result: a <see cref="Result{TValue}"/> is also a <see cref="Result"/>,
/// so code that only cares about success or failure (logging, pipeline behaviors, aggregation)
/// can handle typed and untyped results uniformly.
/// </remarks>
[DebuggerDisplay("{IsSuccess ? \"Success\" : \"Failure: \" + Error.Code + (Errors.Count > 1 ? \" (+\" + (Errors.Count - 1) + \" more)\" : \"\")}")]
public class Result
{

    private readonly ReadOnlyCollection<Error> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the result is successful.</param>
    /// <param name="error">The error, or <see cref="Error.None"/> for a successful result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the success flag and the error disagree.</exception>
    private protected Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        // Reference equality: Error is a record, so an error built with empty
        // code and description would otherwise be indistinguishable from the sentinel.
        var isNone = ReferenceEquals(error, Error.None);

        if (isSuccess && !isNone)
        {
            throw new InvalidOperationException("Invalid result. A successful result cannot have an error.");
        }

        if (!isSuccess && isNone)
        {
            throw new InvalidOperationException("Invalid result. A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        _errors = isSuccess ? ReadOnlyCollection<Error>.Empty : new([error]);
    }

    /// <summary>
    /// Initializes a new failed instance of the <see cref="Result"/> class carrying several errors.
    /// </summary>
    /// <param name="errors">The errors; must contain at least one error and no <see langword="null"/> or <see cref="Error.None"/> entries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors"/> is empty or contains an invalid entry.</exception>
    private protected Result(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var copy = errors.ToArray();

        if (copy.Length == 0)
        {
            throw new ArgumentException("A failed result must have at least one error.", nameof(errors));
        }

        foreach (var error in copy)
        {
            if (error is null || ReferenceEquals(error, Error.None))
            {
                throw new ArgumentException("Errors cannot contain null or Error.None.", nameof(errors));
            }
        }

        IsSuccess = false;
        _errors = new(copy);
    }

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the first error, or <see cref="Error.None"/> when the result is successful.
    /// </summary>
    /// <remarks>Use <see cref="Errors"/> to read every error of a result that failed for several reasons.</remarks>
    public Error Error => IsSuccess ? Error.None : _errors[0];

    /// <summary>
    /// Gets every error of a failed result, in the order they were reported; empty when the result is successful.
    /// </summary>
    public IReadOnlyList<Error> Errors => _errors;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful result with the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed typed result.
    /// </summary>
    /// <typeparam name="TValue">The value type the result would have carried on success.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    /// <summary>
    /// Creates a failed result carrying several errors, e.g. every validation failure of a request.
    /// </summary>
    /// <param name="errors">The errors; at least one is required.</param>
    /// <returns>A failed result with the specified errors.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors"/> is empty or contains <see langword="null"/> or <see cref="Error.None"/>.</exception>
    public static Result Failure(IEnumerable<Error> errors) => new(errors);

    /// <summary>
    /// Creates a failed typed result carrying several errors.
    /// </summary>
    /// <typeparam name="TValue">The value type the result would have carried on success.</typeparam>
    /// <param name="errors">The errors; at least one is required.</param>
    /// <returns>A failed result with the specified errors.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors"/> is empty or contains <see langword="null"/> or <see cref="Error.None"/>.</exception>
    public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => Result<TValue>.Failure(errors);

    /// <summary>
    /// Combines several results: succeeds when all of them succeed, otherwise fails with
    /// the errors of every failed result, in order.
    /// </summary>
    /// <param name="results">The results to combine. Typed results can be passed directly.</param>
    /// <returns>A successful result, or a failure carrying all collected errors.</returns>
    public static Result Combine(params Result[] results) => Combine((IEnumerable<Result>)results);

    /// <inheritdoc cref="Combine(Result[])"/>
    public static Result Combine(IEnumerable<Result> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var errors = results.SelectMany(result => result.Errors).ToArray();

        return errors.Length == 0 ? Success() : new Result(errors);
    }

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);

    /// <summary>
    /// Matches the result and executes the appropriate action.
    /// </summary>
    /// <param name="onSuccess">The action to execute on success.</param>
    /// <param name="onFailure">The action to execute on failure.</param>
    public void Match(Action onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(Error);
        }
    }

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => new(false, error);
}
