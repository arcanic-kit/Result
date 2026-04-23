namespace Arcanic.Result.Tests
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            var result = Result.Success();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
        {
            var result = Result.Success("test");

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal("test", result.Value);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Failure_ShouldCreateFailedResult()
        {
            var error = Error.Failure("Test.Error", "Test error description");

            var result = Result.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Failure_ShouldImplicitlyConvertToTypedResult()
        {
            var error = Error.Failure("Test.Error", "Test error description");

            Result<string> result = Result.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void Value_OnSuccessfulVoidResult_ShouldThrowInvalidOperationException()
        {
            var result = Result.Success();

            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void Value_OnTypedFailure_ShouldThrowInvalidOperationException()
        {
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            Assert.Throws<InvalidOperationException>(() => result.Value);
        }

        [Fact]
        public void ImplicitConversion_FromError_ShouldCreateFailedResult()
        {
            var error = Error.Failure("Test.Error", "Test error description");

            Result result = error;

            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void ImplicitConversion_FromSuccessfulResult_ToTypedResult_ShouldThrow()
        {
            var success = Result.Success();

            Assert.Throws<InvalidOperationException>(() => { Result<string> _ = success; });
        }

        [Fact]
        public void Match_TypedResult_OnSuccess_ShouldExecuteOnSuccess()
        {
            var result = Result.Success("test");
            var executed = false;

            result.Match(
                value => executed = true,
                error => executed = false);

            Assert.True(executed);
        }

        [Fact]
        public void Match_TypedResult_OnFailure_ShouldExecuteOnFailure()
        {
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));
            var executed = false;

            result.Match(
                value => executed = false,
                err => executed = true);

            Assert.True(executed);
        }

        [Fact]
        public void Match_TypedResult_OnSuccess_ShouldReturnMappedValue()
        {
            var result = Result.Success("hello");

            var output = result.Match(
                value => value.Length,
                error => -1);

            Assert.Equal(5, output);
        }

        [Fact]
        public void Match_TypedResult_OnFailure_ShouldReturnFallbackValue()
        {
            Result<string> result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            var output = result.Match(
                value => value.Length,
                err => -1);

            Assert.Equal(-1, output);
        }

        [Fact]
        public void Match_TypedResult_OnFailure_ShouldReceiveCorrectError()
        {
            var error = Error.NotFound("Item.NotFound", "Item was not found");
            Result<string> result = Result.Failure(error);
            Error? capturedError = null;

            result.Match(
                value => { },
                err => capturedError = err);

            Assert.Equal(error, capturedError);
        }

        [Fact]
        public void Match_VoidResult_OnSuccess_ShouldExecuteOnSuccess()
        {
            var result = Result.Success();
            var executed = false;

            result.Match(
                () => executed = true,
                error => executed = false);

            Assert.True(executed);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldExecuteOnFailure()
        {
            Result result = Result.Failure(Error.Failure("Test.Error", "Test error description"));
            var executed = false;

            result.Match(
                () => executed = false,
                err => executed = true);

            Assert.True(executed);
        }

        [Fact]
        public void Match_VoidResult_OnSuccess_ShouldReturnMappedValue()
        {
            var result = Result.Success();

            var output = result.Match(
                () => "success",
                error => "failure");

            Assert.Equal("success", output);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldReturnFallbackValue()
        {
            Result result = Result.Failure(Error.Failure("Test.Error", "Test error description"));

            var output = result.Match(
                () => "success",
                err => "failure");

            Assert.Equal("failure", output);
        }

        [Fact]
        public void Match_VoidResult_OnFailure_ShouldReceiveCorrectError()
        {
            var error = Error.NotFound("Item.NotFound", "Item was not found");
            Result result = Result.Failure(error);
            Error? capturedError = null;

            result.Match(
                () => { },
                err => capturedError = err);

            Assert.Equal(error, capturedError);
        }
    }
}
