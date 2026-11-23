using TaskManager.Application.Common.Results;

namespace TaskManager.Application.Tasks;

public static class TaskErrors
{
    public static readonly Error NotFound = Error.NotFound("Task.NotFound", "Task was not found.");

    public static readonly Error StatusRequired = Error.Validation(
        "Task.StatusRequired", "Status is required. Supported values: Pending, InProgress, Completed.");

    public static readonly Error InvalidStatus = Error.Validation(
        "Task.InvalidStatus", "Status must be one of: Pending, InProgress, Completed.");

    public static readonly Error DueDateRequired = Error.Validation(
        "Task.DueDateRequired", "Due date is required (format YYYY-MM-DD).");

    public static readonly Error InvalidDueDate = Error.Validation(
        "Task.InvalidDueDate", "Due date must be a valid date in the format YYYY-MM-DD.");
}
