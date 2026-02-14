namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">A value indicating whether the result is successful.</param>
    /// <param name="error">The error.</param>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    /// <summary>
    /// Executes the action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute with the value.</param>
    /// <returns>The current result.</returns>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>
    /// Executes the function if the result is successful and returns a new result.
    /// </summary>
    /// <param name="func">The function to execute with the value.</param>
    /// <returns>The result of the function if successful, otherwise the current result.</returns>
    public Result Bind(Func<TValue, Result> func) =>
        IsSuccess ? func(Value) : Failure(Error);

    /// <summary>
    /// Executes the function if the result is successful and returns a new result.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="func">The function to execute with the value.</param>
    /// <returns>The result of the function if successful, otherwise a failed result.</returns>
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> func) =>
        IsSuccess ? func(Value) : Failure<TOut>(Error);

    /// <summary>
    /// Transforms the value using the specified function if successful.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="func">The function to apply to the value.</param>
    /// <returns>The transformed result if successful, otherwise a failed result.</returns>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> func) =>
        IsSuccess ? Success(func(Value)) : Failure<TOut>(Error);

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
    /// </summary>
    /// <param name="value">The value.</param>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}