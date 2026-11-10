namespace Arcanic.Result.Tests
{
    public class MultipleErrorsTests
    {
        private static readonly Error NameEmpty = Error.Validation("Name.Empty", "Name is required");
        private static readonly Error PriceNegative = Error.Validation("Price.Negative", "Price cannot be negative");
        private static readonly Error NotFound = Error.NotFound("Item.NotFound", "Item was not found");

        [Fact]
        public void Success_ShouldHaveNoErrors()
        {
            Assert.Empty(Result.Success().Errors);
            Assert.Empty(Result.Success(1).Errors);
        }

        [Fact]
        public void SingleErrorFailure_ShouldExposeErrorInErrors()
        {
            var result = Result.Failure(NameEmpty);

            Assert.Equal([NameEmpty], result.Errors);
        }

        [Fact]
        public void Failure_WithSeveralErrors_ShouldKeepAllInOrder()
        {
            // Act
            var result = Result.Failure([NameEmpty, PriceNegative]);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal([NameEmpty, PriceNegative], result.Errors);
            Assert.Same(NameEmpty, result.Error);
        }

        [Fact]
        public void TypedFailure_WithSeveralErrors_ShouldKeepAllInOrder()
        {
            // Act
            var result = Result.Failure<int>(new List<Error> { NameEmpty, PriceNegative });

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal([NameEmpty, PriceNegative], result.Errors);
            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Failure_ShouldCopyErrors_SoLaterChangesToTheSourceDoNotLeakIn()
        {
            // Arrange
            var source = new List<Error> { NameEmpty };

            // Act
            var result = Result.Failure(source);
            source.Add(PriceNegative);

            // Assert
            Assert.Equal([NameEmpty], result.Errors);
        }

        [Fact]
        public void Errors_ShouldNotBeMutableThroughACast()
        {
            var result = Result.Failure([NameEmpty, PriceNegative]);

            Assert.False(result.Errors is Error[]);
            Assert.True(((ICollection<Error>)result.Errors).IsReadOnly);
        }

        [Fact]
        public void Failure_WithNoErrors_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure(Array.Empty<Error>()));
        }

        [Fact]
        public void Failure_WithNullErrorList_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Failure((IEnumerable<Error>)null!));
        }

        [Fact]
        public void Failure_WithNullEntry_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure([NameEmpty, null!]));
        }

        [Fact]
        public void Failure_WithNoneEntry_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure([NameEmpty, Error.None]));
        }

        [Fact]
        public void Combine_AllSuccessful_ShouldSucceed()
        {
            // Act
            var result = Result.Combine(Result.Success(), Result.Success("value"), Result.Success(42));

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Combine_WithFailures_ShouldCollectEveryErrorInOrder()
        {
            // Act — typed and untyped results mix freely
            var result = Result.Combine(
                Result.Failure<string>(NameEmpty),
                Result.Success(42),
                Result.Failure([PriceNegative, NotFound]));

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal([NameEmpty, PriceNegative, NotFound], result.Errors);
        }

        [Fact]
        public void Combine_Empty_ShouldSucceed()
        {
            Assert.True(Result.Combine().IsSuccess);
            Assert.True(Result.Combine(Enumerable.Empty<Result>()).IsSuccess);
        }

        [Fact]
        public void Match_OnMultiErrorFailure_ShouldReceiveFirstError()
        {
            var result = Result.Failure([NameEmpty, PriceNegative]);

            var code = result.Match(() => "ok", error => error.Code);

            Assert.Equal(NameEmpty.Code, code);
        }
    }
}
