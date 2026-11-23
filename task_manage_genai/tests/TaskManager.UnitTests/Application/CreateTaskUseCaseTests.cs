using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;
using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.Application;

public class CreateTaskUseCaseTests
{
    private readonly TaskTestContext _ctx = new();

    private CreateTaskUseCase CreateSut(ICurrentUserContext? currentUser = null)
        => new(currentUser ?? _ctx.CurrentUserContext, _ctx.Users, _ctx.Tasks);

    [Fact]
    public async Task Execute_WithValidCommand_PersistsTaskOwnedByCurrentUser()
    {
        var result = await CreateSut().ExecuteAsync(
            new CreateTaskCommand("Write report", "Q4", "InProgress", "2026-12-31"));

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_ctx.Tasks.Tasks);
        Assert.Equal(_ctx.CurrentUser.Id, stored.UserId);
        Assert.Equal(stored.Id, result.Value.Id);
        Assert.Equal("Write report", result.Value.Title);
        Assert.Equal("Q4", result.Value.Description);
        Assert.Equal(TaskItemStatus.InProgress, result.Value.Status);
        Assert.Equal(new DateOnly(2026, 12, 31), result.Value.DueDate);
    }

    [Fact]
    public async Task Execute_WithoutStatus_DefaultsToPending()
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, null, "2026-12-31"));

        Assert.True(result.IsSuccess);
        Assert.Equal(TaskItemStatus.Pending, result.Value.Status);
    }

    [Theory]
    [InlineData("pending", TaskItemStatus.Pending)]
    [InlineData("INPROGRESS", TaskItemStatus.InProgress)]
    [InlineData("Completed", TaskItemStatus.Completed)]
    public async Task Execute_AcceptsStatusNamesCaseInsensitively(string status, TaskItemStatus expected)
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, status, "2026-12-31"));

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Status);
    }

    [Fact]
    public async Task Execute_WithUnknownUser_ReturnsUserNotFound()
    {
        var sut = CreateSut(new FakeCurrentUserContext(Guid.NewGuid()));

        var result = await sut.ExecuteAsync(new CreateTaskCommand("Title", null, null, "2026-12-31"));

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
        Assert.Empty(_ctx.Tasks.Tasks);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Execute_WithBlankTitle_LetsDomainExceptionPropagate(string? title)
    {
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            CreateSut().ExecuteAsync(new CreateTaskCommand(title, null, null, "2026-12-31")));

        Assert.Equal("Task.TitleRequired", ex.Code);
        Assert.Empty(_ctx.Tasks.Tasks);
    }

    [Theory]
    [InlineData("Done")]
    [InlineData("1")]
    [InlineData("Pending, Completed")]
    [InlineData("")]
    public async Task Execute_WithUnsupportedStatus_ReturnsValidationError(string status)
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, status, "2026-12-31"));

        AssertFailedWith(result, TaskErrors.InvalidStatus);
    }

    [Fact]
    public async Task Execute_WithoutDueDate_ReturnsValidationError()
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, null, null));

        AssertFailedWith(result, TaskErrors.DueDateRequired);
    }

    [Theory]
    [InlineData("31/12/2026")]
    [InlineData("2026-13-01")]
    [InlineData("2026-02-30")]
    [InlineData("2026-1-5")]
    [InlineData("2026-12-31T00:00:00")]
    [InlineData("tomorrow")]
    [InlineData(" ")]
    public async Task Execute_WithInvalidDueDateFormat_ReturnsValidationError(string dueDate)
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, null, dueDate));

        AssertFailedWith(result, TaskErrors.InvalidDueDate);
    }

    [Fact]
    public async Task Execute_WithPastDueDate_Succeeds()
    {
        var result = await CreateSut().ExecuteAsync(new CreateTaskCommand("Title", null, null, "1999-01-01"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(1999, 1, 1), result.Value.DueDate);
    }

    private void AssertFailedWith(Result<TaskDto> result, Error expected)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(expected, result.Error);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(_ctx.Tasks.Tasks);
    }
}
