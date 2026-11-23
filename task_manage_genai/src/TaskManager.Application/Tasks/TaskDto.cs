using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

public sealed record TaskDto(Guid Id, string Title, string? Description, TaskItemStatus Status, DateOnly DueDate)
{
    public static TaskDto From(TaskItem task)
        => new(task.Id, task.Title, task.Description, task.Status, task.DueDate);
}

public sealed record CreateTaskCommand(string? Title, string? Description, string? Status, string? DueDate);

public sealed record UpdateTaskCommand(string? Title, string? Description, string? Status, string? DueDate);
