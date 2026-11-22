using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Infrastructure.Persistence
{
    public sealed class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _factory;
        public DatabaseInitializer(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                PRAGMA journal_mode = WAL;

                CREATE TABLE IF NOT EXISTS Users (
                    Id    TEXT PRIMARY KEY,
                    Name  TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS Books (
                    Id         TEXT PRIMARY KEY,
                    OwnerId    TEXT NOT NULL,
                    Title      TEXT NOT NULL,
                    Author     TEXT NOT NULL,
                    Status     INTEGER NOT NULL,
                    Rating     INTEGER NULL CHECK (Rating BETWEEN 1 AND 5),
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NULL,
                    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
                );

                CREATE INDEX IF NOT EXISTS IX_Books_OwnerId ON Books(OwnerId);
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
