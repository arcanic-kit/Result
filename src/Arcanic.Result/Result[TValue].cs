namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
[DebuggerDisplay("{IsSuccess ? \"Success: \" + Value : \"Failure: \" + Error.Code}")]
public class Result<TValue>
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">A value indicating whether the result is successful.</param>
    /// <param name="error">The error.</param>
    internal Result(TValue? value, bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Invalid result. A successful result cannot have an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Invalid result. A failed result must have an error.");
        }

        _value = value;
        IsSuccess = isSuccess;
        Error = error;
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
    /// Gets the error.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful result with the specified value.</returns>
    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>
    /// Matches the result and executes the appropriate function.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">The function to execute on success with the value.</param>
    /// <param name="onFailure">The function to execute on failure with the error.</param>
    /// <returns>The result of the executed function.</returns>
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    /// Matches the result and executes the appropriate action.
    /// </summary>
    /// <param name="onSuccess">The action to execute on success with the value.</param>
    /// <param name="onFailure">The action to execute on failure with the error.</param>
    public void Match(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Error);
        }
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// A <see langword="null"/> value produces a failed result with <see cref="Error.None"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful result containing <paramref name="value"/>, or a failed result if <paramref name="value"/> is <see langword="null"/>.</returns>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure(Error.None);

    /// <summary>
    /// Implicitly converts a failed <see cref="Result"/> to a typed failed result,
    /// allowing <see cref="Result.Failure"/> to be returned from methods that return <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="result">The failed result to convert.</param>
    /// <returns>A typed failed result carrying the same error.</returns>
    public static implicit operator Result<TValue>(Result result) => Failure(result.Error);
}
