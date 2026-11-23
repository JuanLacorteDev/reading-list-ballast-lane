namespace TaskManager.Infrastructure.Persistence;

/// <summary>Creates the schema if it does not exist yet. Safe to run on every startup.</summary>
public sealed class DatabaseInitializer(SqliteConnectionProvider connectionProvider)
{
    // ids are GUIDs stored as TEXT; status is the stable numeric TaskItemStatus value;
    // due_date is an ISO date (YYYY-MM-DD) stored as TEXT.
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS users (
            id   TEXT NOT NULL PRIMARY KEY,
            name TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS tasks (
            id          TEXT    NOT NULL PRIMARY KEY,
            user_id     TEXT    NOT NULL REFERENCES users (id),
            title       TEXT    NOT NULL,
            description TEXT    NULL,
            status      INTEGER NOT NULL,
            due_date    TEXT    NOT NULL
        );

        CREATE INDEX IF NOT EXISTS ix_tasks_user_id ON tasks (user_id);
        """;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await connectionProvider.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = Schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
