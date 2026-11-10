namespace Arcanic.Result.Tests
{
    public class ResultExtensionsTests
    {
        private static readonly Error NotFound = Error.NotFound("Item.NotFound", "Item was not found");
        private static readonly Error TooSmall = Error.Validation("Number.TooSmall", "Number is too small");
        private static readonly Error Other = Error.Validation("Other", "Other error");

        private static Result<int> Parse(string input) =>
            int.TryParse(input, out var n) ? n : Error.Validation("INVALID_NUMBER", "Not a number");

        // ---- Map ----------------------------------------------------------

        [Fact]
        public void Map_OnSuccess_ShouldTransformValue()
        {
            var result = Result.Success(21).Map(x => x * 2);

            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Map_OnFailure_ShouldSkipAndKeepAllErrors()
        {
            var called = false;
            var source = Result.Failure<int>([NotFound, Other]);

            var result = source.Map(x => { called = true; return x.ToString(); });

            Assert.False(called);
            Assert.Equal([NotFound, Other], result.Errors);
        }

        [Fact]
        public void Map_OnUntypedSuccess_ShouldProduceValue()
        {
            Assert.Equal("done", Result.Success().Map(() => "done").Value);
        }

        [Fact]
        public void Map_OnUntypedFailure_ShouldPropagateFailure()
        {
            Result source = NotFound;

            Assert.Same(NotFound, source.Map(() => "done").Error);
        }

        // ---- Bind ---------------------------------------------------------

        [Fact]
        public void Bind_OnSuccess_ShouldChainNextOperation()
        {
            var result = Result.Success("42").Bind(Parse);

            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void Bind_WhenNextOperationFails_ShouldReturnItsFailure()
        {
            var result = Result.Success("abc").Bind(Parse);

            Assert.Equal("INVALID_NUMBER", result.Error.Code);
        }

        [Fact]
        public void Bind_OnFailure_ShouldSkipAndKeepAllErrors()
        {
            var called = false;

            var result = Result.Failure<string>([NotFound, Other])
                .Bind(s => { called = true; return Parse(s); });

            Assert.False(called);
            Assert.Equal([NotFound, Other], result.Errors);
        }

        [Fact]
        public void Bind_ToUntypedResult_ShouldChain()
        {
            Result Check(int n) => n > 0 ? Result.Success() : TooSmall;

            Assert.True(Result.Success(1).Bind(Check).IsSuccess);
            Assert.Same(TooSmall, Result.Success(0).Bind(Check).Error);
        }

        [Fact]
        public void Bind_WithLambdaReturningError_ShouldResolveToUntypedBind()
        {
            // The lambda's natural type is Error, which only converts to Result.
            Result result = Result.Success(0).Bind(n => TooSmall);

            Assert.Same(TooSmall, result.Error);
        }

        [Fact]
        public void Bind_FromUntypedResult_ShouldChain()
        {
            Assert.Equal(7, Result.Success().Bind(() => Result.Success(7)).Value);
            Assert.True(Result.Success().Bind(() => Result.Success()).IsSuccess);

            Result failed = NotFound;
            Assert.Same(NotFound, failed.Bind(() => Result.Success(7)).Error);
            Assert.Same(NotFound, failed.Bind(() => Result.Success()).Error);
        }

        // ---- Ensure -------------------------------------------------------

        [Fact]
        public void Ensure_WhenPredicateHolds_ShouldKeepResult()
        {
            var source = Result.Success(5);

            Assert.Same(source, source.Ensure(n => n > 0, TooSmall));
        }

        [Fact]
        public void Ensure_WhenPredicateFails_ShouldReturnError()
        {
            var result = Result.Success(-1).Ensure(n => n > 0, TooSmall);

            Assert.Same(TooSmall, result.Error);
        }

        [Fact]
        public void Ensure_OnFailure_ShouldNotEvaluatePredicate()
        {
            var called = false;
            var source = Result.Failure<int>(NotFound);

            var result = source.Ensure(n => { called = true; return true; }, TooSmall);

            Assert.False(called);
            Assert.Same(NotFound, result.Error);
        }

        // ---- Tap / TapError -----------------------------------------------

        [Fact]
        public void Tap_OnSuccess_ShouldRunSideEffectAndReturnSameResult()
        {
            var seen = 0;
            var source = Result.Success(3);

            var result = source.Tap(n => seen = n);

            Assert.Equal(3, seen);
            Assert.Same(source, result);
        }

        [Fact]
        public void Tap_WithoutValue_ShouldPreserveTypedResultType()
        {
            var called = false;

            Result<int> result = Result.Success(3).Tap(() => called = true);

            Assert.True(called);
            Assert.Equal(3, result.Value);
        }

        [Fact]
        public void Tap_OnFailure_ShouldNotRun()
        {
            var called = false;

            Result.Failure<int>(NotFound).Tap(_ => called = true);
            Result.Failure(NotFound).Tap(() => called = true);

            Assert.False(called);
        }

        [Fact]
        public void TapError_OnFailure_ShouldReceiveEveryError()
        {
            IReadOnlyList<Error>? seen = null;
            var source = Result.Failure<int>([NotFound, Other]);

            var result = source.TapError(errors => seen = errors);

            Assert.Equal([NotFound, Other], seen);
            Assert.Same(source, result);
        }

        [Fact]
        public void TapError_OnSuccess_ShouldNotRun()
        {
            var called = false;

            Result.Success(1).TapError(_ => called = true);

            Assert.False(called);
        }

        // ---- Pipelines ----------------------------------------------------

        [Theory]
        [InlineData("10", true, "20")]
        [InlineData("abc", false, "INVALID_NUMBER")]
        [InlineData("0", false, "Number.TooSmall")]
        public void Pipeline_ShouldStopAtFirstFailure(string input, bool expectSuccess, string expected)
        {
            var log = new List<string>();

            var result = Result.Success(input)
                .Bind(Parse)
                .Ensure(n => n > 0, TooSmall)
                .Map(n => n * 2)
                .Tap(n => log.Add($"ok {n}"))
                .TapError(errors => log.Add($"failed {errors[0].Code}"))
                .Map(n => n.ToString());

            Assert.Equal(expectSuccess, result.IsSuccess);
            Assert.Equal(expected, result.IsSuccess ? result.Value : result.Error.Code);
            Assert.Single(log);
        }

        // ---- Argument validation ------------------------------------------

        [Fact]
        public void Combinators_WithNullArguments_ShouldThrowArgumentNullException()
        {
            var ok = Result.Success(1);

            Assert.Throws<ArgumentNullException>(() => ok.Map((Func<int, int>)null!));
            Assert.Throws<ArgumentNullException>(() => ok.Bind((Func<int, Result<int>>)null!));
            Assert.Throws<ArgumentNullException>(() => ok.Ensure(null!, TooSmall));
            Assert.Throws<ArgumentNullException>(() => ok.Ensure(_ => true, null!));
            Assert.Throws<ArgumentNullException>(() => ok.Tap((Action<int>)null!));
            Assert.Throws<ArgumentNullException>(() => ((Result<int>)null!).Map(x => x));
        }
    }
}
