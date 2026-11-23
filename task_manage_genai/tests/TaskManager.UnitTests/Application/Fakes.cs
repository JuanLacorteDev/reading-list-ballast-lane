using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.UnitTests.Application;

internal sealed class FakeCurrentUserContext(Guid userId) : ICurrentUserContext
{
    public Guid UserId { get; } = userId;
}

internal sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Users.Any(u => u.Id == userId));
}

internal sealed class InMemoryTaskRepository : ITaskRepository
{
    public List<TaskItem> Tasks { get; } = [];

    public Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        Tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task<TaskItem?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Tasks.SingleOrDefault(t => t.Id == taskId && t.UserId == userId));

    public Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TaskItem>>(Tasks.Where(t => t.UserId == userId).ToList());

    public Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
        => Task.FromResult(Tasks.Any(t => t.Id == task.Id && t.UserId == task.UserId));

    public Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(Tasks.RemoveAll(t => t.Id == taskId && t.UserId == userId) > 0);
}

/// <summary>Shared arrangement: one existing "current" user and one other existing user.</summary>
internal sealed class TaskTestContext
{
    public InMemoryUserRepository Users { get; } = new();
    public InMemoryTaskRepository Tasks { get; } = new();
    public User CurrentUser { get; } = User.Create("Current");
    public User OtherUser { get; } = User.Create("Other");
    public ICurrentUserContext CurrentUserContext { get; }

    public TaskTestContext()
    {
        Users.Users.AddRange([CurrentUser, OtherUser]);
        CurrentUserContext = new FakeCurrentUserContext(CurrentUser.Id);
    }

    public TaskItem SeedTask(User owner, string title = "Seeded")
    {
        var task = TaskItem.Create(owner.Id, title, null, TaskItemStatus.Pending, new DateOnly(2026, 12, 31));
        Tasks.Tasks.Add(task);
        return task;
    }
}
