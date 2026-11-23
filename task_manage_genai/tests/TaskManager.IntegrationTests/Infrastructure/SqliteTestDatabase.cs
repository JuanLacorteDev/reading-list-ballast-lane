using Microsoft.Data.Sqlite;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.IntegrationTests.Infrastructure;

/// <summary>A throwaway SQLite file database, initialized with the real schema.</summary>
public sealed class SqliteTestDatabase : IAsyncLifetime
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"taskmanager-tests-{Guid.NewGuid():N}.db");

    public string ConnectionString => $"Data Source={_path};Foreign Keys=True";
    public SqliteConnectionProvider ConnectionProvider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        ConnectionProvider = new SqliteConnectionProvider(ConnectionString);
        await new DatabaseInitializer(ConnectionProvider).InitializeAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path)) File.Delete(_path);
        return Task.CompletedTask;
    }

    /// <summary>Runs a raw scalar query, for asserting on what is physically stored.</summary>
    public async Task<object?> ScalarAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = await ConnectionProvider.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        return await command.ExecuteScalarAsync();
    }
}
