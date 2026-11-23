using System.Globalization;
using TaskManager.Application.Common.Results;
using TaskManager.Domain.Tasks;

namespace TaskManager.Application.Tasks;

/// <summary>Converts raw client input (status name, YYYY-MM-DD date) into domain types.</summary>
internal static class TaskInputParser
{
    /// <param name="defaultStatus">Used when <paramref name="value"/> is null; null means status is required.</param>
    public static Result<TaskItemStatus> ParseStatus(string? value, TaskItemStatus? defaultStatus = null)
    {
        if (value is null)
            return defaultStatus is { } fallback ? fallback : TaskErrors.StatusRequired;

        // Name match only: Enum.TryParse would also accept "1" or "Pending, Completed".
        foreach (var status in Enum.GetValues<TaskItemStatus>())
        {
            if (string.Equals(status.ToString(), value, StringComparison.OrdinalIgnoreCase))
                return status;
        }

        return TaskErrors.InvalidStatus;
    }

    public static Result<DateOnly> ParseDueDate(string? value)
    {
        if (value is null)
            return TaskErrors.DueDateRequired;

        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : TaskErrors.InvalidDueDate;
    }
}
