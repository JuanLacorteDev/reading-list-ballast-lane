using Microsoft.Data.Sqlite;
using ReadingList.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Infrastructure.Tests.Repositories
{
    public sealed class SqliteTestDatabase : IAsyncLifetime
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"readinglist-tests-{Guid.NewGuid()}.db");

        public IDbConnectionFactory Factory { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Factory = new SqliteConnectionFactory($"Data Source={_path};Foreign Keys=True");
            await new DatabaseInitializer(Factory).InitializeAsync();
        }

        public Task DisposeAsync()
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_path);
            return Task.CompletedTask;
        }
    }
}
