namespace TaskManager.Domain.Tasks;

/// <summary>
/// Task lifecycle status. Numeric values are persisted and MUST remain stable:
/// never renumber or reuse a value; append new members with new numbers.
/// </summary>
public enum TaskItemStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
}
