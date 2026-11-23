using System.Globalization;
using Microsoft.Data.Sqlite;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Tasks;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// ADO.NET task repository. Every value is passed as a SQL parameter, and every
/// single-task statement is scoped by both task id and owner id.
/// </summary>
public sealed class SqliteTaskRepository(SqliteConnectionProvider connectionProvider) : ITaskRepository
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string SelectColumns = "SELECT id, user_id, title, description, status, due_date FROM tasks";

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO tasks (id, user_id, title, description, status, due_date)
            VALUES ($id, $user_id, $title, $description, $status, $due_date);
            """;
        AddTaskParameters(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"{SelectColumns} WHERE id = $id AND user_id = $user_id;";
        command.Parameters.AddWithValue("$id", taskId.ToString());
        command.Parameters.AddWithValue("$user_id", userId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"{SelectColumns} WHERE user_id = $user_id ORDER BY due_date, title;";
        command.Parameters.AddWithValue("$user_id", userId.ToString());

        var tasks = new List<TaskItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            tasks.Add(Map(reader));
        return tasks;
    }

    public async Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE tasks
               SET title = $title, description = $description, status = $status, due_date = $due_date
             WHERE id = $id AND user_id = $user_id;
            """;
        AddTaskParameters(command, task);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tasks WHERE id = $id AND user_id = $user_id;";
        command.Parameters.AddWithValue("$id", taskId.ToString());
        command.Parameters.AddWithValue("$user_id", userId.ToString());
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static void AddTaskParameters(SqliteCommand command, TaskItem task)
    {
        command.Parameters.AddWithValue("$id", task.Id.ToString());
        command.Parameters.AddWithValue("$user_id", task.UserId.ToString());
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", (int)task.Status);
        command.Parameters.AddWithValue("$due_date", task.DueDate.ToString(DateFormat, CultureInfo.InvariantCulture));
    }

    private static TaskItem Map(SqliteDataReader reader) => TaskItem.Restore(
        id: Guid.Parse(reader.GetString(0)),
        userId: Guid.Parse(reader.GetString(1)),
        title: reader.GetString(2),
        description: reader.IsDBNull(3) ? null : reader.GetString(3),
        status: (TaskItemStatus)reader.GetInt32(4),
        dueDate: DateOnly.ParseExact(reader.GetString(5), DateFormat, CultureInfo.InvariantCulture));
}
