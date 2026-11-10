namespace Arcanic.Result.Tests
{
    public class ErrorTests
    {
        public static TheoryData<ErrorType, Func<string, string, Error>> Factories => new()
        {
            { ErrorType.Failure, Error.Failure },
            { ErrorType.Validation, Error.Validation },
            { ErrorType.Conflict, Error.Conflict },
            { ErrorType.NotFound, Error.NotFound },
            { ErrorType.Unauthorized, Error.Unauthorized },
            { ErrorType.Forbidden, Error.Forbidden },
        };

        [Theory]
        [MemberData(nameof(Factories))]
        public void Factory_ShouldCreateErrorWithMatchingType(ErrorType expectedType, Func<string, string, Error> factory)
        {
            // Arrange
            var code = $"Test.{expectedType}";
            var description = $"Test {expectedType} description";

            // Act
            var error = factory(code, description);

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.Equal(expectedType, error.Type);
        }

        [Fact]
        public void Factories_ShouldCoverEveryErrorType()
        {
            var covered = Factories.Select(row => (ErrorType)row[0]).ToHashSet();

            Assert.Equal(Enum.GetValues<ErrorType>().ToHashSet(), covered);
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
        public void None_ShouldAlwaysBeTheSameInstance()
        {
            Assert.Same(Error.None, Error.None);
            Assert.Same(Error.None, Result.Success().Error);
            Assert.Same(Error.None, Result.Success(1).Error);
        }

        [Fact]
        public void Error_SameValues_ShouldBeEqual()
        {
            // Arrange
            var a = Error.Failure("Test.Error", "Test description");
            var b = Error.Failure("Test.Error", "Test description");

            // Assert
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
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
