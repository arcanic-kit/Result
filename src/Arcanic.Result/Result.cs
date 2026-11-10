using System.ComponentModel;

namespace Arcanic.Result;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
[DebuggerDisplay("{IsSuccess ? \"Success\" : \"Failure: \" + Error.Code}")]
public class Result : Result<Result>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the result is successful.</param>
    /// <param name="error">The error.</param>
    internal Result(bool isSuccess, Error error)
        : base(default, isSuccess, error)
    {
    }

    /// <summary>
    /// This property is not supported on a void result. Use <see cref="Result{TValue}"/> for operations that return a value.
    /// </summary>
    /// <remarks>
    /// Overrides (rather than hides) the base property so it also throws when accessed
    /// through a <see cref="Result{TValue}"/> reference.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    public override Result Value =>
        throw new InvalidOperationException(NoValueMessage);

    private const string NoValueMessage =
        "Result does not carry a value. Use Result<TValue> for operations that return a value.";

    private const string ValueMatchMessage =
        "Result does not carry a value; use the Match overload whose onSuccess takes no parameters.";

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
    public new static Result Failure(Error error) => new(false, error);

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
    /// Not supported: hides the inherited value-based overload, which would otherwise compile
    /// and pass a meaningless value to <paramref name="onSuccess"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    [Obsolete(ValueMatchMessage, error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new TOut Match<TOut>(Func<Result, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        throw new NotSupportedException(ValueMatchMessage);

    /// <summary>
    /// Not supported: hides the inherited value-based overload, which would otherwise compile
    /// and pass a meaningless value to <paramref name="onSuccess"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    [Obsolete(ValueMatchMessage, error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new void Match(Action<Result> onSuccess, Action<Error> onFailure) =>
        throw new NotSupportedException(ValueMatchMessage);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => new(false, error);
}
