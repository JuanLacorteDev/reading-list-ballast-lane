using TaskManager.Application.Abstractions;
using TaskManager.Application.Common.Results;
using TaskManager.Application.Users;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed class CreateTaskUseCase(ICurrentUserContext currentUser, IUserRepository users, ITaskRepository tasks)
{
    public async Task<Result<TaskDto>> ExecuteAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
            return UserErrors.NotFound;

        var status = TaskInputParser.ParseStatus(command.Status, defaultStatus: TaskItemStatus.Pending);
        if (status.IsFailure) return status.Error!;

        var dueDate = TaskInputParser.ParseDueDate(command.DueDate);
        if (dueDate.IsFailure) return dueDate.Error!;

        var task = TaskItem.Create(currentUser.UserId, command.Title!, command.Description, status.Value, dueDate.Value);
        await tasks.AddAsync(task, cancellationToken);
        return TaskDto.From(task);
    }
}

public sealed class ListTasksUseCase(ICurrentUserContext currentUser, IUserRepository users, ITaskRepository tasks)
{
    public async Task<Result<IReadOnlyList<TaskDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
            return UserErrors.NotFound;

        var items = await tasks.ListByUserAsync(currentUser.UserId, cancellationToken);
        return items.Select(TaskDto.From).ToList();
    }
}

public sealed class GetTaskUseCase(ICurrentUserContext currentUser, IUserRepository users, ITaskRepository tasks)
{
    public async Task<Result<TaskDto>> ExecuteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
            return UserErrors.NotFound;

        var task = await tasks.GetByIdAsync(taskId, currentUser.UserId, cancellationToken);
        if (task is null) return TaskErrors.NotFound;

        return TaskDto.From(task);
    }
}

public sealed class UpdateTaskUseCase(ICurrentUserContext currentUser, IUserRepository users, ITaskRepository tasks)
{
    public async Task<Result<TaskDto>> ExecuteAsync(Guid taskId, UpdateTaskCommand command, CancellationToken cancellationToken = default)
    {
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
            return UserErrors.NotFound;

        var task = await tasks.GetByIdAsync(taskId, currentUser.UserId, cancellationToken);
        if (task is null) return TaskErrors.NotFound;

        var status = TaskInputParser.ParseStatus(command.Status);
        if (status.IsFailure) return status.Error!;

        var dueDate = TaskInputParser.ParseDueDate(command.DueDate);
        if (dueDate.IsFailure) return dueDate.Error!;

        task.Update(command.Title!, command.Description, status.Value, dueDate.Value);

        // The row may have been deleted between the read and the write.
        if (!await tasks.UpdateAsync(task, cancellationToken))
            return TaskErrors.NotFound;

        return TaskDto.From(task);
    }
}

public sealed class DeleteTaskUseCase(ICurrentUserContext currentUser, IUserRepository users, ITaskRepository tasks)
{
    public async Task<Result> ExecuteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
            return UserErrors.NotFound;

        return await tasks.DeleteAsync(taskId, currentUser.UserId, cancellationToken)
            ? Result.Success()
            : TaskErrors.NotFound;
    }
}
