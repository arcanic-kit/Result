namespace Arcanic.Result.Tests
{
    public class FailureResultTests
    {
        [Fact]
        public void FailureResult_IsFailure_ShouldBeTrue()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            FailureResult failureResult = Result.Failure(error);

            // Assert
            Assert.True(failureResult.IsFailure);
        }

        [Fact]
        public void FailureResult_IsSuccess_ShouldBeFalse()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            FailureResult failureResult = Result.Failure(error);

            // Assert
            Assert.False(failureResult.IsSuccess);
        }

        [Fact]
        public void FailureResult_Error_ShouldMatchProvidedError()
        {
            // Arrange
            var error = Error.NotFound("Product.NotFound", "Product was not found");
            FailureResult failureResult = Result.Failure(error);

            // Assert
            Assert.Equal(error, failureResult.Error);
        }

        [Fact]
        public void FailureResult_Match_Generic_ShouldReturnFallbackValue()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            FailureResult failureResult = Result.Failure(error);

            // Act
            var output = failureResult.Match(
                () => "success",
                err => "failure");

            // Assert
            Assert.Equal("failure", output);
        }

        [Fact]
        public void FailureResult_Match_Generic_ShouldReceiveCorrectError()
        {
            // Arrange
            var error = Error.Validation("Name.Empty", "Name cannot be empty");
            FailureResult failureResult = Result.Failure(error);
            Error? capturedError = null;

            // Act
            failureResult.Match(
                () => "success",
                err => { capturedError = err; return "failure"; });

            // Assert
            Assert.Equal(error, capturedError);
        }

        [Fact]
        public void FailureResult_Match_Action_ShouldExecuteOnFailure()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            FailureResult failureResult = Result.Failure(error);
            var executed = false;

            // Act
            failureResult.Match(
                () => executed = false,
                err => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void FailureResult_ImplicitConversion_ToResult_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.Conflict("Order.Duplicate", "Order already exists");
            FailureResult failureResult = Result.Failure(error);

            // Act
            Result result = failureResult;

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void FailureResult_ImplicitConversion_ToTypedResult_ShouldCreateFailedResult()
        {
            // Arrange
            var error = Error.NotFound("User.NotFound", "User was not found");
            FailureResult failureResult = Result.Failure(error);

            // Act
            Result<Guid> result = failureResult;

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void FailureResult_Value_WhenConvertedToTypedResult_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var error = Error.Failure("Test.Error", "Test error description");
            FailureResult failureResult = Result.Failure(error);
            Result<int> result = failureResult;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }
    }
}
