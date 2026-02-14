namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the result is successful.</param>
    /// <param name="error">The error.</param>
    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Invalid result. A successful result cannot have an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Invalid result. A failed result must have an error.");
        }

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
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a value type.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a result that succeeds if the condition is true, otherwise fails with the specified error.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="error">The error to use if the condition is false.</param>
    /// <returns>A successful result if the condition is true, otherwise a failed result.</returns>
    public static Result Ensure(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    /// Creates a result that succeeds if the condition function returns true, otherwise fails with the specified error.
    /// </summary>
    /// <param name="condition">The condition function to evaluate.</param>
    /// <param name="error">The error to use if the condition is false.</param>
    /// <returns>A successful result if the condition is true, otherwise a failed result.</returns>
    public static Result Ensure(Func<bool> condition, Error error) =>
        Ensure(condition(), error);

    /// <summary>
    /// Creates a result based on whether the value is null.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="error">The error to use if the value is null.</param>
    /// <returns>A successful result if the value is not null, otherwise a failed result.</returns>
    public static Result<TValue> Create<TValue>(TValue? value, Error error)
        where TValue : class =>
        value is not null ? Success(value) : Failure<TValue>(error);

    /// <summary>
    /// Creates a result based on whether the nullable value has a value.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="value">The nullable value to check.</param>
    /// <param name="error">The error to use if the value is null.</param>
    /// <returns>A successful result if the value has a value, otherwise a failed result.</returns>
    public static Result<TValue> Create<TValue>(TValue? value, Error error)
        where TValue : struct =>
        value.HasValue ? Success(value.Value) : Failure<TValue>(error);

    /// <summary>
    /// Executes the action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The current result.</returns>
    public Result Tap(Action action)
    {
        if (IsSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Executes the function if the result is successful and returns a new result.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function if successful, otherwise the current result.</returns>
    public Result Bind(Func<Result> func) =>
        IsSuccess ? func() : this;

    /// <summary>
    /// Transforms the result using the specified function if successful.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="func">The function to apply.</param>
    /// <returns>The transformed result if successful, otherwise a failed result.</returns>
    public Result<TOut> Map<TOut>(Func<TOut> func) =>
        IsSuccess ? Success(func()) : Failure<TOut>(Error);

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
    public static implicit operator Result(Error error) => Failure(error);
}