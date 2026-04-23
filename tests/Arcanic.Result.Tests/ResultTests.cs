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
            // Act
            var result = Result.Success("test");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal("test", result.Value);
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
        public void Failure_ShouldImplicitlyConvertToTypedResult()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");

            // Act
            Result<string> result = Result.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Value_OnSuccessfulVoidResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var result = Result.Success();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Value_OnTypedFailure_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => result.Value);
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
        public void ImplicitConversion_FromSuccessfulResult_ToTypedResult_ShouldThrow()
        {
            // Arrange
            var success = Result.Success();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { Result<string> _ = success; });
        }

        [Fact]
        public void Match_TypedResult_OnSuccess_ShouldExecuteOnSuccess()
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
        public void Match_TypedResult_OnFailure_ShouldExecuteOnFailure()
        {
            // Arrange
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));
            var executed = false;

            // Act
            result.Match(
                value => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_TypedResult_OnSuccess_ShouldReturnMappedValue()
        {
            // Arrange
            var result = Result.Success("hello");

            // Act
            var output = result.Match(
                value => value.Length,
                error => -1);

            // Assert
            Assert.Equal(5, output);
        }

        [Fact]
        public void Match_TypedResult_OnFailure_ShouldReturnFallbackValue()
        {
            // Arrange
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            // Act
            var output = result.Match(
                value => value.Length,
                err => -1);

            // Assert
            Assert.Equal(-1, output);
        }

        [Fact]
        public void Match_TypedResult_OnFailure_ShouldReceiveCorrectError()
        {
            // Arrange
            var error = Error.NotFound("Item.NotFound", "Item was not found");
            Result<string> result = Result.Failure(error);
            Error? capturedError = null;

            // Act
            result.Match(
                value => { },
                err => capturedError = err);

            // Assert
            Assert.Equal(error, capturedError);
        }

        [Fact]
        public void Match_VoidResult_OnSuccess_ShouldExecuteOnSuccess()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            result.Match(
                () => executed = true,
                error => executed = false);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldExecuteOnFailure()
        {
            // Arrange
            Result result = Result.Failure(Error.Failure("Test.Error", "Test error description"));
            var executed = false;

            // Act
            result.Match(
                () => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_VoidResult_OnSuccess_ShouldReturnMappedValue()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var output = result.Match(
                () => "success",
                error => "failure");

            // Assert
            Assert.Equal("success", output);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldReturnFallbackValue()
        {
            // Arrange
            Result result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            // Act
            var output = result.Match(
                () => "success",
                err => "failure");

            // Assert
            Assert.Equal("failure", output);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldReceiveCorrectError()
        {
            // Arrange
            var error = Error.NotFound("Item.NotFound", "Item was not found");
            Result result = Result.Failure(error);
            Error? capturedError = null;

            // Act
            result.Match(
                () => { },
                err => capturedError = err);

            // Assert
            Assert.Equal(error, capturedError);
        }
    }
}
