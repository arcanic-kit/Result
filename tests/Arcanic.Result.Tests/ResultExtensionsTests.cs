namespace Arcanic.Result.Tests
{
    public class ResultExtensionsTests
    {
        [Fact]
        public void Ensure_WhenResultIsSuccessAndConditionTrue_ShouldReturnSuccess()
        {
            // Arrange
            var result = Result.Success();
            var error = Error.Failure("Test.Error", "Test error");

            // Act
            var ensuredResult = result.Ensure(() => true, error);

            // Assert
            Assert.True(ensuredResult.IsSuccess);
        }

        [Fact]
        public void Ensure_WhenResultIsSuccessAndConditionFalse_ShouldReturnFailure()
        {
            // Arrange
            var result = Result.Success();
            var error = Error.Failure("Test.Error", "Test error");

            // Act
            var ensuredResult = result.Ensure(() => false, error);

            // Assert
            Assert.False(ensuredResult.IsSuccess);
            Assert.Equal(error, ensuredResult.Error);
        }

        [Fact]
        public void Ensure_WhenResultIsFailure_ShouldReturnOriginalFailure()
        {
            // Arrange
            var originalError = Error.Failure("Original.Error", "Original error");
            var result = Result.Failure(originalError);
            var newError = Error.Failure("New.Error", "New error");

            // Act
            var ensuredResult = result.Ensure(() => true, newError);

            // Assert
            Assert.False(ensuredResult.IsSuccess);
            Assert.Equal(originalError, ensuredResult.Error);
        }

        [Fact]
        public void EnsureWithValue_WhenConditionTrue_ShouldReturnSuccess()
        {
            // Arrange
            var result = Result.Success("test");
            var error = Error.Failure("Test.Error", "Test error");

            // Act
            var ensuredResult = result.Ensure(value => value.Length > 2, error);

            // Assert
            Assert.True(ensuredResult.IsSuccess);
            Assert.Equal("test", ensuredResult.Value);
        }

        [Fact]
        public void EnsureWithValue_WhenConditionFalse_ShouldReturnFailure()
        {
            // Arrange
            var result = Result.Success("a");
            var error = Error.Failure("Test.Error", "Test error");

            // Act
            var ensuredResult = result.Ensure(value => value.Length > 2, error);

            // Assert
            Assert.False(ensuredResult.IsSuccess);
            Assert.Equal(error, ensuredResult.Error);
        }

        [Fact]
        public void Combine_WhenAllResultsAreSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var result1 = Result.Success();
            var result2 = Result.Success();
            var result3 = Result.Success();

            // Act
            var combinedResult = ResultExtensions.Combine(result1, result2, result3);

            // Assert
            Assert.True(combinedResult.IsSuccess);
        }

        [Fact]
        public void Combine_WhenOneResultIsFailure_ShouldReturnFirstFailure()
        {
            // Arrange
            var result1 = Result.Success();
            var error = Error.Failure("Test.Error", "Test error");
            var result2 = Result.Failure(error);
            var result3 = Result.Success();

            // Act
            var combinedResult = ResultExtensions.Combine(result1, result2, result3);

            // Assert
            Assert.False(combinedResult.IsSuccess);
            Assert.Equal(error, combinedResult.Error);
        }

        [Fact]
        public void Try_WhenActionSucceeds_ShouldReturnSuccess()
        {
            // Arrange
            var executed = false;

            // Act
            var result = ResultExtensions.Try(() => executed = true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(executed);
        }

        [Fact]
        public void Try_WhenActionThrowsException_ShouldReturnFailure()
        {
            // Act
            var result = ResultExtensions.Try(() => throw new InvalidOperationException("Test exception"));

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Exception", result.Error.Code);
            Assert.Equal("Test exception", result.Error.Description);
        }

        [Fact]
        public void TryWithFunc_WhenFunctionSucceeds_ShouldReturnSuccessWithValue()
        {
            // Act
            var result = ResultExtensions.Try(() => "test");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test", result.Value);
        }

        [Fact]
        public void TryWithFunc_WhenFunctionThrowsException_ShouldReturnFailure()
        {
            // Act
            var result = ResultExtensions.Try<string>(() => throw new InvalidOperationException("Test exception"));

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Exception", result.Error.Code);
            Assert.Equal("Test exception", result.Error.Description);
        }

        [Fact]
        public async Task TryAsync_WhenActionSucceeds_ShouldReturnSuccess()
        {
            // Arrange
            var executed = false;

            // Act
            var result = await ResultExtensions.TryAsync(async () =>
            {
                await Task.Delay(1);
                executed = true;
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(executed);
        }

        [Fact]
        public async Task TryAsync_WhenActionThrowsException_ShouldReturnFailure()
        {
            // Act
            var result = await ResultExtensions.TryAsync(async () =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Exception", result.Error.Code);
            Assert.Equal("Test exception", result.Error.Description);
        }

        [Fact]
        public async Task TryAsyncWithFunc_WhenFunctionSucceeds_ShouldReturnSuccessWithValue()
        {
            // Act
            var result = await ResultExtensions.TryAsync(async () =>
            {
                await Task.Delay(1);
                return "test";
            });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test", result.Value);
        }

        [Fact]
        public async Task BindAsync_WhenResultIsSuccess_ShouldExecuteFunction()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            var newResult = await result.BindAsync(async () =>
            {
                await Task.Delay(1);
                executed = true;
                return Result.Success();
            });

            // Assert
            Assert.True(executed);
            Assert.True(newResult.IsSuccess);
        }

        [Fact]
        public async Task BindAsync_WhenResultIsFailure_ShouldNotExecuteFunction()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error");
            var result = Result.Failure(error);
            var executed = false;

            // Act
            var newResult = await result.BindAsync(async () =>
            {
                await Task.Delay(1);
                executed = true;
                return Result.Success();
            });

            // Assert
            Assert.False(executed);
            Assert.False(newResult.IsSuccess);
            Assert.Equal(error, newResult.Error);
        }

        [Fact]
        public async Task MapAsync_WhenResultIsSuccess_ShouldTransformValue()
        {
            // Arrange
            var result = Result.Success("test");

            // Act
            var mappedResult = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                return value.ToUpper();
            });

            // Assert
            Assert.True(mappedResult.IsSuccess);
            Assert.Equal("TEST", mappedResult.Value);
        }

        [Fact]
        public async Task MapAsync_WhenResultIsFailure_ShouldReturnFailedResult()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error");
            var result = Result.Failure<string>(error);

            // Act
            var mappedResult = await result.MapAsync(async value =>
            {
                await Task.Delay(1);
                return value.ToUpper();
            });

            // Assert
            Assert.False(mappedResult.IsSuccess);
            Assert.Equal(error, mappedResult.Error);
        }
    }
}