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
        public void None_ShouldEqualAnotherNoneInstance()
        {
            Assert.Equal(Error.None, Error.None);
        }

        [Fact]
        public void Error_SameValues_ShouldBeEqual()
        {
            // Arrange
            var a = Error.Failure("Test.Error", "Test description");
            var b = Error.Failure("Test.Error", "Test description");

            // Assert
            Assert.Equal(a, b);
        }

        [Fact]
        public void Error_DifferentCode_ShouldNotBeEqual()
        {
            // Arrange
            var a = Error.Failure("Error.A", "Same description");
            var b = Error.Failure("Error.B", "Same description");

            // Assert
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Error_DifferentType_ShouldNotBeEqual()
        {
            // Arrange
            var a = Error.Failure("Test.Error", "Same description");
            var b = Error.Validation("Test.Error", "Same description");

            // Assert
            Assert.NotEqual(a, b);
        }
    }
}