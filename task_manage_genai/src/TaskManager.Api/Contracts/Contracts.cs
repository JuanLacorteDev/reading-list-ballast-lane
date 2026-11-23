using TaskManager.Application.Tasks;

namespace TaskManager.Api.Contracts;

// JSON property names are snake_case (see Program.cs). Task requests intentionally have no
// user_id: the owner always comes from the current-user context. Status and due_date are
// received as strings so the use case can report unsupported values with specific errors.

public sealed record CreateUserRequest(string? Name);

public sealed record CreateUserResponse(Guid Id);

public sealed record CreateTaskRequest(string? Title, string? Description, string? Status, string? DueDate)
{
    public CreateTaskCommand ToCommand() => new(Title, Description, Status, DueDate);
}

public sealed record UpdateTaskRequest(string? Title, string? Description, string? Status, string? DueDate)
{
    public UpdateTaskCommand ToCommand() => new(Title, Description, Status, DueDate);
}

public sealed record TaskResponse(Guid Id, string Title, string? Description, string Status, DateOnly DueDate)
{
    public static TaskResponse From(TaskDto task)
        => new(task.Id, task.Title, task.Description, task.Status.ToString(), task.DueDate);
}
