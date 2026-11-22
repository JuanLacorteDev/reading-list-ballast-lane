using ReadingList.Api.Common.Errors;
using ReadingList.Application.Common.Results;

namespace ReadingList.Api.Tests.Errors;

public sealed class ErrorStatusCodesTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    public void From_MapsEveryErrorType(ErrorType type, int expected)
    {
        Assert.Equal(expected, ErrorStatusCodes.From(type));
    }

    [Fact]
    public void From_CoversAllErrorTypes()
    {
        // Fails when someone adds an ErrorType and forgets to map it.
        foreach (var type in Enum.GetValues<ErrorType>())
            Assert.NotEqual(500, ErrorStatusCodes.From(type));
    }
}
