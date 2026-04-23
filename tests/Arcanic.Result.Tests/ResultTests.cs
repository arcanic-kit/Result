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
            Result<string> result = Result.Failure(error);

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
            Result<string> result = Result.Failure(error);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Success_WithValue_GenericResult_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = Result.Success("test");

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
            Result<string> result = Result.Failure(error);
            var executed = false;

            // Act
            result.Match(
                value => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_Generic_WithSuccessResult_ShouldReturnMappedValue()
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
        public void Match_Generic_WithFailureResult_ShouldReturnFallbackValue()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            Result<string> result = Result.Failure(error);

            // Act
            var output = result.Match(
                value => value.Length,
                err => -1);

            // Assert
            Assert.Equal(-1, output);
        }

        [Fact]
        public void Match_Action_NonGenericResult_WithSuccessResult_ShouldExecuteOnSuccess()
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
        public void Match_Action_NonGenericResult_WithFailureResult_ShouldExecuteOnFailure()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            Result result = Result.Failure(error);
            var executed = false;

            // Act
            result.Match(
                () => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void Match_Generic_NonGenericResult_WithSuccessResult_ShouldReturnMappedValue()
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
        public void Match_Generic_NonGenericResult_WithFailureResult_ShouldReturnFallbackValue()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            Result result = Result.Failure(error);

            // Act
            var output = result.Match(
                () => "success",
                err => "failure");

            // Assert
            Assert.Equal("failure", output);
        }

        [Fact]
        public void Failure_ImplicitToTypedResult_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.NotFound("Product.NotFound", "Product was not found");

            // Act
            Result<string> result = Result.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Match_WithFailureResult_ShouldReceiveCorrectError()
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
    }
}
