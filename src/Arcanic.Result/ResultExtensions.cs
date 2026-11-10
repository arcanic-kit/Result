namespace Arcanic.Result;

/// <summary>
/// Railway-oriented combinators for chaining operations on <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
/// <remarks>
/// Every combinator runs its function only on success. On failure the function is skipped and
/// the failure is passed along unchanged, with all of its <see cref="Result.Errors"/>.
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Transforms the value of a successful result.
    /// </summary>
    /// <typeparam name="TIn">The value type of the source result.</typeparam>
    /// <typeparam name="TOut">The value type of the returned result.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="map">The transformation, applied on success.</param>
    /// <returns>A result carrying the transformed value, or the original failure.</returns>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        return result.IsSuccess
            ? Result.Success(map(result.Value))
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>
    /// Produces a value after a successful untyped result.
    /// </summary>
    /// <typeparam name="TOut">The value type of the returned result.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="map">The value factory, invoked on success.</param>
    /// <returns>A result carrying the produced value, or the original failure.</returns>
    public static Result<TOut> Map<TOut>(this Result result, Func<TOut> map)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        return result.IsSuccess
            ? Result.Success(map())
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>
    /// Chains an operation that can itself fail, passing it the value of a successful result.
    /// </summary>
    /// <typeparam name="TIn">The value type of the source result.</typeparam>
    /// <typeparam name="TOut">The value type of the returned result.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="bind">The next operation, invoked on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure.</returns>
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? bind(result.Value)
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>
    /// Chains an operation without a value that can itself fail, passing it the value of a successful result.
    /// </summary>
    /// <typeparam name="TIn">The value type of the source result.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="bind">The next operation, invoked on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure.</returns>
    public static Result Bind<TIn>(this Result<TIn> result, Func<TIn, Result> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? bind(result.Value)
            : Result.Failure(result.Errors);
    }

    /// <summary>
    /// Chains an operation that can itself fail after a successful untyped result.
    /// </summary>
    /// <typeparam name="TOut">The value type of the returned result.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="bind">The next operation, invoked on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure.</returns>
    public static Result<TOut> Bind<TOut>(this Result result, Func<Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? bind()
            : Result.Failure<TOut>(result.Errors);
    }

    /// <summary>
    /// Chains an operation without a value that can itself fail after a successful untyped result.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <param name="bind">The next operation, invoked on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure.</returns>
    public static Result Bind(this Result result, Func<Result> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        return result.IsSuccess
            ? bind()
            : Result.Failure(result.Errors);
    }

    /// <summary>
    /// Turns a successful result into a failure when its value does not satisfy a condition.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="predicate">The condition the value must satisfy, checked on success.</param>
    /// <param name="error">The error returned when the condition is not met.</param>
    /// <returns>The original result, or a failure carrying <paramref name="error"/>.</returns>
    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(error);

        if (result.IsFailure || predicate(result.Value))
        {
            return result;
        }

        return Result.Failure<TValue>(error);
    }

    /// <summary>
    /// Runs a side effect (logging, metrics, ...) with the value of a successful result.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The side effect, invoked on success.</param>
    /// <returns>The original result.</returns>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Runs a side effect when a result is successful.
    /// </summary>
    /// <typeparam name="TResult">The result type, preserved in the return value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The side effect, invoked on success.</param>
    /// <returns>The original result.</returns>
    public static TResult Tap<TResult>(this TResult result, Action action)
        where TResult : Result
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action();
        }

        return result;
    }

    /// <summary>
    /// Runs a side effect with every error when a result has failed.
    /// </summary>
    /// <typeparam name="TResult">The result type, preserved in the return value.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="action">The side effect, invoked on failure with <see cref="Result.Errors"/>.</param>
    /// <returns>The original result.</returns>
    public static TResult TapError<TResult>(this TResult result, Action<IReadOnlyList<Error>> action)
        where TResult : Result
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailure)
        {
            action(result.Errors);
        }

        return result;
    }
}
