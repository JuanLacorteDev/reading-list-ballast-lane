using Microsoft.Data.Sqlite;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.IntegrationTests.Infrastructure;

public sealed class SqliteRepositoryTests : IAsyncLifetime
{
    private readonly SqliteTestDatabase _db = new();
    private SqliteUserRepository _users = null!;
    private SqliteTaskRepository _tasks = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _users = new SqliteUserRepository(_db.ConnectionProvider);
        _tasks = new SqliteTaskRepository(_db.ConnectionProvider);
    }

    public Task DisposeAsync() => _db.DisposeAsync();

    private async Task<User> AddUserAsync(string name = "Ada")
    {
        var user = User.Create(name);
        await _users.AddAsync(user);
        return user;
    }

    private async Task<TaskItem> AddTaskAsync(User owner, string title = "Task", DateOnly? dueDate = null)
    {
        var task = TaskItem.Create(owner.Id, title, "desc", TaskItemStatus.Pending, dueDate ?? new DateOnly(2026, 12, 31));
        await _tasks.AddAsync(task);
        return task;
    }

    [Fact]
    public async Task Users_AddedUserExists_UnknownUserDoesNot()
    {
        var user = await AddUserAsync();

        Assert.True(await _users.ExistsAsync(user.Id));
        Assert.False(await _users.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Tasks_RoundTripAllFields()
    {
        var user = await AddUserAsync();
        var task = TaskItem.Create(user.Id, "Write report", "Quarterly", TaskItemStatus.InProgress, new DateOnly(2026, 2, 28));
        await _tasks.AddAsync(task);

        var loaded = await _tasks.GetByIdAsync(task.Id, user.Id);

        Assert.NotNull(loaded);
        Assert.Equal(task.Id, loaded.Id);
        Assert.Equal(user.Id, loaded.UserId);
        Assert.Equal("Write report", loaded.Title);
        Assert.Equal("Quarterly", loaded.Description);
        Assert.Equal(TaskItemStatus.InProgress, loaded.Status);
        Assert.Equal(new DateOnly(2026, 2, 28), loaded.DueDate);
    }

    [Fact]
    public async Task Tasks_NullDescriptionRoundTrips()
    {
        var user = await AddUserAsync();
        var task = TaskItem.Create(user.Id, "No description", null, TaskItemStatus.Pending, new DateOnly(2026, 1, 1));
        await _tasks.AddAsync(task);

        var loaded = await _tasks.GetByIdAsync(task.Id, user.Id);

        Assert.Null(loaded!.Description);
    }

    [Fact]
    public async Task Tasks_StatusIsStoredAsStableNumber_AndDueDateAsIsoDate()
    {
        var user = await AddUserAsync();
        var task = TaskItem.Create(user.Id, "Title", null, TaskItemStatus.Completed, new DateOnly(2026, 3, 5));
        await _tasks.AddAsync(task);

        var status = await _db.ScalarAsync("SELECT status FROM tasks WHERE id = $id", ("$id", task.Id.ToString()));
        var dueDate = await _db.ScalarAsync("SELECT due_date FROM tasks WHERE id = $id", ("$id", task.Id.ToString()));

        Assert.Equal(3L, status);
        Assert.Equal("2026-03-05", dueDate);
    }

    [Fact]
    public async Task Tasks_ForeignKeyRejectsTaskForUnknownUser()
    {
        var orphan = TaskItem.Create(Guid.NewGuid(), "Orphan", null, TaskItemStatus.Pending, new DateOnly(2026, 1, 1));

        var ex = await Assert.ThrowsAsync<SqliteException>(() => _tasks.AddAsync(orphan));

        Assert.Equal(19, ex.SqliteErrorCode); // SQLITE_CONSTRAINT
    }

    [Fact]
    public async Task Tasks_GetById_ScopedByOwner()
    {
        var owner = await AddUserAsync("Owner");
        var other = await AddUserAsync("Other");
        var task = await AddTaskAsync(owner);

        Assert.NotNull(await _tasks.GetByIdAsync(task.Id, owner.Id));
        Assert.Null(await _tasks.GetByIdAsync(task.Id, other.Id));
        Assert.Null(await _tasks.GetByIdAsync(Guid.NewGuid(), owner.Id));
    }

    [Fact]
    public async Task Tasks_ListByUser_ReturnsOnlyOwnTasksOrderedByDueDate()
    {
        var owner = await AddUserAsync("Owner");
        var other = await AddUserAsync("Other");
        var later = await AddTaskAsync(owner, "Later", new DateOnly(2027, 1, 1));
        var sooner = await AddTaskAsync(owner, "Sooner", new DateOnly(2026, 1, 1));
        await AddTaskAsync(other, "Theirs");

        var list = await _tasks.ListByUserAsync(owner.Id);

        Assert.Equal([sooner.Id, later.Id], list.Select(t => t.Id));
    }

    [Fact]
    public async Task Tasks_Update_PersistsChangesForOwner()
    {
        var owner = await AddUserAsync();
        var task = await AddTaskAsync(owner);

        task.Update("Updated", null, TaskItemStatus.Completed, new DateOnly(2027, 6, 1));
        var updated = await _tasks.UpdateAsync(task);

        Assert.True(updated);
        var loaded = await _tasks.GetByIdAsync(task.Id, owner.Id);
        Assert.Equal("Updated", loaded!.Title);
        Assert.Null(loaded.Description);
        Assert.Equal(TaskItemStatus.Completed, loaded.Status);
        Assert.Equal(new DateOnly(2027, 6, 1), loaded.DueDate);
    }

    [Fact]
    public async Task Tasks_Update_WithDifferentOwner_DoesNothing()
    {
        var owner = await AddUserAsync("Owner");
        var other = await AddUserAsync("Other");
        var task = await AddTaskAsync(owner, "Original");

        var forged = TaskItem.Restore(task.Id, other.Id, "Hijacked", null, TaskItemStatus.Completed, new DateOnly(2027, 1, 1));
        var updated = await _tasks.UpdateAsync(forged);

        Assert.False(updated);
        Assert.Equal("Original", (await _tasks.GetByIdAsync(task.Id, owner.Id))!.Title);
    }

    [Fact]
    public async Task Tasks_Delete_OnlyForOwner()
    {
        var owner = await AddUserAsync("Owner");
        var other = await AddUserAsync("Other");
        var task = await AddTaskAsync(owner);

        Assert.False(await _tasks.DeleteAsync(task.Id, other.Id));
        Assert.NotNull(await _tasks.GetByIdAsync(task.Id, owner.Id));

        Assert.True(await _tasks.DeleteAsync(task.Id, owner.Id));
        Assert.Null(await _tasks.GetByIdAsync(task.Id, owner.Id));
    }

    [Theory]
    [InlineData("'); DROP TABLE tasks; --")]
    [InlineData("x' OR '1'='1")]
    [InlineData("\"; DELETE FROM users; --")]
    public async Task SqlInjectionLikeInput_IsStoredAsPlainData(string payload)
    {
        var user = await AddUserAsync(payload);
        var task = TaskItem.Create(user.Id, payload, payload, TaskItemStatus.Pending, new DateOnly(2026, 1, 1));
        await _tasks.AddAsync(task);

        var loaded = await _tasks.GetByIdAsync(task.Id, user.Id);
        Assert.Equal(payload, loaded!.Title);
        Assert.Equal(payload, loaded.Description);
        Assert.Equal(payload, await _db.ScalarAsync("SELECT name FROM users WHERE id = $id", ("$id", user.Id.ToString())));

        Assert.Equal(1L, await _db.ScalarAsync("SELECT COUNT(*) FROM tasks"));
        Assert.Equal(1L, await _db.ScalarAsync("SELECT COUNT(*) FROM users"));
    }

    [Fact]
    public async Task Initializer_IsIdempotent()
    {
        var user = await AddUserAsync();

        await new DatabaseInitializer(_db.ConnectionProvider).InitializeAsync();

        Assert.True(await _users.ExistsAsync(user.Id));
    }
}
