using Microsoft.Data.Sqlite;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// Opens SQLite connections. Foreign-key enforcement is expected to be enabled through the
/// connection string (<c>Foreign Keys=True</c>), as configured in appsettings.json.
/// </summary>
public sealed class SqliteConnectionProvider(string connectionString)
{
    public async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
