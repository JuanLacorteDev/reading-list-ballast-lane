using TaskManager.Api.Errors;
using TaskManager.Application.Common.Results;

namespace TaskManager.UnitTests.Api;

public class ErrorStatusCodesTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    public void From_MapsEachErrorTypeToItsStatusCode(ErrorType type, int expected)
        => Assert.Equal(expected, ErrorStatusCodes.From(type));

    [Fact]
    public void From_HasAMappingForEveryErrorType()
    {
        // Fails when a new ErrorType is added without a mapping in ErrorStatusCodes.
        foreach (var type in Enum.GetValues<ErrorType>())
        {
            var exception = Record.Exception(() => ErrorStatusCodes.From(type));
            Assert.True(exception is null, $"ErrorType.{type} has no HTTP status mapping.");
        }
    }
}
