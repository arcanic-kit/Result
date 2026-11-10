namespace Arcanic.Result.Tests
{
    public class ResultAsyncExtensionsTests
    {
        private static readonly Error NotFound = Error.NotFound("User.NotFound", "User was not found");
        private static readonly Error Inactive = Error.Conflict("User.Inactive", "User is inactive");
        private static readonly Error Other = Error.Validation("Other", "Other error");

        private sealed record User(int Id, string Name, bool IsActive);

        private static readonly Dictionary<int, User> Users = new()
        {
            [1] = new User(1, "Ada", IsActive: true),
            [2] = new User(2, "Bob", IsActive: false),
        };

        private static async Task<Result<User>> FindUserAsync(int id)
        {
            await Task.Yield();
            return Users.TryGetValue(id, out var user) ? user : NotFound;
        }

        private static async Task<Result> SendWelcomeAsync(User user)
        {
            await Task.Yield();
            return Result.Success();
        }

        private static Task<Result<int>> Async(Result<int> result) => Task.FromResult(result);

        // ---- Async functions on a result ----------------------------------

        [Fact]
        public async Task MapAsync_OnSuccess_ShouldAwaitTransformation()
        {
            var result = await Result.Success(2).MapAsync(async n => { await Task.Yield(); return n * 10; });

            Assert.Equal(20, result.Value);
        }

        [Fact]
        public async Task MapAsync_OnFailure_ShouldSkipAndKeepAllErrors()
        {
            var called = false;

            var result = await Result.Failure<int>([NotFound, Other])
                .MapAsync(n => { called = true; return Task.FromResult(n); });

            Assert.False(called);
            Assert.Equal([NotFound, Other], result.Errors);
        }

        [Fact]
        public async Task BindAsync_WithMethodGroup_ShouldChain()
        {
            Assert.Equal("Ada", (await Result.Success(1).BindAsync(FindUserAsync)).Value.Name);
            Assert.Same(NotFound, (await Result.Success(99).BindAsync(FindUserAsync)).Error);
        }

        [Fact]
        public async Task BindAsync_ToUntypedResult_ShouldChain()
        {
            var result = await Result.Success(Users[1]).BindAsync(SendWelcomeAsync);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task BindAsync_WithAsyncLambdaReturningTypedResult_ShouldPreferTypedOverload()
        {
            Result<string> result = await Result.Success(1).BindAsync(async n =>
            {
                await Task.Yield();
                return Result.Success(n.ToString());
            });

            Assert.Equal("1", result.Value);
        }

        [Fact]
        public async Task BindAsync_FromUntypedResult_ShouldChain()
        {
            Result failed = NotFound;

            Assert.True((await Result.Success().BindAsync(() => Task.FromResult(Result.Success()))).IsSuccess);
            Assert.Same(NotFound, (await failed.BindAsync(() => Task.FromResult(Result.Success()))).Error);
        }

        [Fact]
        public async Task EnsureAsync_ShouldApplyAsyncPredicate()
        {
            Assert.True((await Result.Success(Users[1]).EnsureAsync(u => Task.FromResult(u.IsActive), Inactive)).IsSuccess);
            Assert.Same(Inactive, (await Result.Success(Users[2]).EnsureAsync(u => Task.FromResult(u.IsActive), Inactive)).Error);
        }

        [Fact]
        public async Task TapAsync_ShouldRunOnlyOnSuccess()
        {
            var seen = new List<int>();

            await Result.Success(1).TapAsync(n => { seen.Add(n); return Task.CompletedTask; });
            await Result.Failure<int>(NotFound).TapAsync(n => { seen.Add(n); return Task.CompletedTask; });

            Assert.Equal([1], seen);
        }

        // ---- Sync functions on a task of a result -------------------------

        [Fact]
        public async Task TaskMap_ShouldTransformAfterAwaiting()
        {
            Assert.Equal(6, (await Async(3).Map(n => n * 2)).Value);
        }

        [Fact]
        public async Task TaskBind_ShouldChainAfterAwaiting()
        {
            Assert.Equal(Inactive, (await Async(2).Bind(n => n > 2 ? Result.Success(n) : Inactive)).Error);
            Assert.True((await Async(3).Bind(n => Result.Success())).IsSuccess);
        }

        [Fact]
        public async Task TaskBind_OnUntypedTask_ShouldChain()
        {
            Assert.True((await Task.FromResult(Result.Success()).Bind(() => Result.Success())).IsSuccess);
        }

        [Fact]
        public async Task TaskEnsure_ShouldApplyPredicateAfterAwaiting()
        {
            Assert.Same(Other, (await Async(-1).Ensure(n => n > 0, Other)).Error);
        }

        [Fact]
        public async Task TaskTapAndTapError_ShouldPreserveResultType()
        {
            var log = new List<string>();

            Result<int> ok = await Async(1).Tap(n => log.Add($"value {n}")).Tap(() => log.Add("ok"));
            Result<int> failed = await Async(Result.Failure<int>([NotFound, Other]))
                .TapError(errors => log.Add($"errors {errors.Count}"));

            Assert.Equal(1, ok.Value);
            Assert.True(failed.IsFailure);
            Assert.Equal(["value 1", "ok", "errors 2"], log);
        }

        [Fact]
        public async Task TaskMatch_ShouldMatchAfterAwaiting()
        {
            Assert.Equal("ok 1", await Async(1).Match(n => $"ok {n}", e => e.Code));
            Assert.Equal("Other", await Async(Other).Match(n => $"ok {n}", e => e.Code));
            Assert.Equal("ok", await Task.FromResult(Result.Success()).Match(() => "ok", e => e.Code));
        }

        // ---- Async functions on a task of a result ------------------------

        [Theory]
        [InlineData(1, "Welcome sent to Ada")]
        [InlineData(2, "User.Inactive")]
        [InlineData(99, "User.NotFound")]
        public async Task Pipeline_ShouldComposeAcrossAwaits(int id, string expected)
        {
            var outcome = await FindUserAsync(id)
                .Ensure(user => user.IsActive, Inactive)
                .TapAsync(user => Task.CompletedTask)
                .MapAsync(user => Task.FromResult(user.Name))
                .BindAsync(name => Task.FromResult(Result.Success($"Welcome sent to {name}")))
                .EnsureAsync(message => Task.FromResult(message.Length > 0), Other)
                .Match(message => message, error => error.Code);

            Assert.Equal(expected, outcome);
        }

        [Fact]
        public async Task TaskBindAsync_ToUntypedResult_ShouldChain()
        {
            var result = await FindUserAsync(1).BindAsync(SendWelcomeAsync);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task TaskBindAsync_OnUntypedTask_ShouldChain()
        {
            var result = await Task.FromResult(Result.Success()).BindAsync(() => Task.FromResult<Result>(Other));

            Assert.Same(Other, result.Error);
        }

        [Fact]
        public async Task NullTask_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => ((Task<Result<int>>)null!).Map(n => n));
        }
    }
}
