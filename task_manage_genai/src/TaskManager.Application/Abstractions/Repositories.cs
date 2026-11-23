using TaskManager.Domain.Tasks;
using TaskManager.Domain.Users;

namespace TaskManager.Application.Abstractions;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Task persistence. Every single-task operation is scoped by BOTH task id and owner id,
/// so a task belonging to another user is indistinguishable from a missing one.
/// </summary>
public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> when no task with that id exists for the task's owner.</returns>
    Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <returns><c>false</c> when no task with that id exists for the given owner.</returns>
    Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
