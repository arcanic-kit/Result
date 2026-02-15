namespace Arcanic.Result.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = Result.Success();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
        {
            // Arrange
            const string expectedValue = "test";

            // Act
            var result = Result.Success(expectedValue);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(expectedValue, result.Value);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Failure_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");

            // Act
            var result = Result.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Failure_WithValueType_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");

            // Act
            var result = Result.Failure<string>(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Value_WhenFailure_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            var result = Result.Failure<string>(error);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
        {
            // Act
            Result<string> result = "test";

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test", result.Value);
        }

        [Fact]
        public void ImplicitConversion_FromError_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");

            // Act
            Result result = error;

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Match_WithSuccessResult_ShouldExecuteOnSuccess()
        {
            // Arrange
            var result = Result.Success("test");
            var executed = false;

            // Act
            result.Match(
                value => executed = true,
                error => executed = false);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_WithFailureResult_ShouldExecuteOnFailure()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            var result = Result.Failure<string>(error);
            var executed = false;

            // Act
            result.Match(
                value => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }
    }
}
