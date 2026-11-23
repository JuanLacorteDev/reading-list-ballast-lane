using TaskManager.Domain.Common;

namespace TaskManager.Domain.Tasks;

/// <summary>A task owned by a single user. Broken business rules throw <see cref="DomainException"/>.</summary>
public sealed class TaskItem
{
    private TaskItem(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateOnly dueDate)
    {
        if (id == Guid.Empty)
            throw new DomainException("Task.IdRequired", "Task id must not be empty.");
        if (userId == Guid.Empty)
            throw new DomainException("Task.OwnerRequired", "Task owner must not be empty.");

        Id = id;
        UserId = userId;
        Apply(title, description, status, dueDate);
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateOnly DueDate { get; private set; }

    public static TaskItem Create(Guid userId, string title, string? description, TaskItemStatus status, DateOnly dueDate)
        => new(Guid.NewGuid(), userId, title, description, status, dueDate);

    /// <summary>Rebuilds an existing task from persisted state.</summary>
    public static TaskItem Restore(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateOnly dueDate)
        => new(id, userId, title, description, status, dueDate);

    public void Update(string title, string? description, TaskItemStatus status, DateOnly dueDate)
        => Apply(title, description, status, dueDate);

    private void Apply(string title, string? description, TaskItemStatus status, DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task.TitleRequired", "Title is required and must not be blank.");
        if (!Enum.IsDefined(status))
            throw new DomainException("Task.StatusInvalid", "Status must be one of: Pending, InProgress, Completed.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = status;
        DueDate = dueDate;
    }
}
