using TaskManager.Domain.Common;
using TaskManager.Domain.Tasks;

namespace TaskManager.UnitTests.Domain;

public class TaskItemTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly DueDate = new(2026, 12, 31);

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        var task = TaskItem.Create(UserId, "  Write report  ", "Quarterly", TaskItemStatus.InProgress, DueDate);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(UserId, task.UserId);
        Assert.Equal("Write report", task.Title);
        Assert.Equal("Quarterly", task.Description);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(DueDate, task.DueDate);
    }

    [Fact]
    public void Create_AllowsDueDateInThePast()
    {
        var past = new DateOnly(2000, 1, 1);

        var task = TaskItem.Create(UserId, "Old task", null, TaskItemStatus.Completed, past);

        Assert.Equal(past, task.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankTitle_Throws(string? title)
    {
        Assert.Throws<DomainException>(() =>
            TaskItem.Create(UserId, title!, null, TaskItemStatus.Pending, DueDate));
    }

    [Fact]
    public void Create_WithBlankTitle_ExposesStableCode()
    {
        var ex = Assert.Throws<DomainException>(() =>
            TaskItem.Create(UserId, " ", null, TaskItemStatus.Pending, DueDate));

        Assert.Equal("Task.TitleRequired", ex.Code);
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        Assert.Throws<DomainException>(() =>
            TaskItem.Create(Guid.Empty, "Title", null, TaskItemStatus.Pending, DueDate));
    }

    [Fact]
    public void Create_WithUndefinedStatus_Throws()
    {
        Assert.Throws<DomainException>(() =>
            TaskItem.Create(UserId, "Title", null, (TaskItemStatus)99, DueDate));
    }

    [Fact]
    public void Create_WithWhitespaceDescription_StoresNull()
    {
        var task = TaskItem.Create(UserId, "Title", "   ", TaskItemStatus.Pending, DueDate);

        Assert.Null(task.Description);
    }

    [Fact]
    public void Update_ChangesMutableFields_ButKeepsIdentityAndOwner()
    {
        var task = TaskItem.Create(UserId, "Title", null, TaskItemStatus.Pending, DueDate);
        var id = task.Id;

        task.Update("New title", "New description", TaskItemStatus.Completed, new DateOnly(2027, 1, 15));

        Assert.Equal(id, task.Id);
        Assert.Equal(UserId, task.UserId);
        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.Equal(new DateOnly(2027, 1, 15), task.DueDate);
    }

    [Fact]
    public void Update_WithBlankTitle_Throws()
    {
        var task = TaskItem.Create(UserId, "Title", null, TaskItemStatus.Pending, DueDate);

        Assert.Throws<DomainException>(() =>
            task.Update(" ", null, TaskItemStatus.Pending, DueDate));
    }

    [Fact]
    public void Status_HasExplicitStableNumericValues()
    {
        // These values are persisted in SQLite; changing them breaks existing records.
        Assert.Equal(1, (int)TaskItemStatus.Pending);
        Assert.Equal(2, (int)TaskItemStatus.InProgress);
        Assert.Equal(3, (int)TaskItemStatus.Completed);
    }
}
