namespace Arcanic.Result.Tests
{
    public class ErrorTests
    {
        [Fact]
        public void Failure_ShouldCreateErrorWithFailureType()
        {
            // Arrange
            const string code = "Test.Failure";
            const string description = "Test failure description";

            // Act
            var error = Error.Failure(code, description);

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(ErrorType.Failure, error.Type);
        }

        [Fact]
        public void Validation_ShouldCreateErrorWithValidationType()
        {
            // Arrange
            const string code = "Test.Validation";
            const string description = "Test validation description";

            // Act
            var error = Error.Validation(code, description);

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(ErrorType.Validation, error.Type);
        }

        [Fact]
        public void Conflict_ShouldCreateErrorWithConflictType()
        {
            // Arrange
            const string code = "Test.Conflict";
            const string description = "Test conflict description";

            // Act
            var error = Error.Conflict(code, description);

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(ErrorType.Conflict, error.Type);
        }

        [Fact]
        public void NotFound_ShouldCreateErrorWithNotFoundType()
        {
            // Arrange
            const string code = "Test.NotFound";
            const string description = "Test not found description";

            // Act
            var error = Error.NotFound(code, description);

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(ErrorType.NotFound, error.Type);
        }

        [Fact]
        public void None_ShouldHaveEmptyValues()
        {
            // Act
            var error = Error.None;

            // Assert
            Assert.Equal(string.Empty, error.Code);
            Assert.Equal(string.Empty, error.Description);
            Assert.Equal(ErrorType.Failure, error.Type);
        }

        [Fact]
        public void NullValue_ShouldHaveExpectedValues()
        {
            // Act
            var error = Error.NullValue;

            // Assert
            Assert.Equal("Error.NullValue", error.Code);
            Assert.Equal("The specified result value is null.", error.Description);
            Assert.Equal(ErrorType.Failure, error.Type);
        }
    }
}