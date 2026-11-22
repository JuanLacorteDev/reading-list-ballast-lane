using Microsoft.Data.Sqlite;
using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ReadingList.Infrastructure.Persistence.Users
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _factory;
        public UserRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Users (Id, Name, Email, PasswordHash, CreatedAt)
                VALUES ($id, $name, $email, $passwordHash, $createdAt);
                """;
                
            command.Parameters.AddWithValue("$id", user.Id.ToString());
            command.Parameters.AddWithValue("$name", user.Name);
            command.Parameters.AddWithValue("$email", user.Email.ToString());
            command.Parameters.AddWithValue("$passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("O", CultureInfo.InvariantCulture));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id FROM Users WHERE Email = $email;";
            command.Parameters.AddWithValue("$email", email.ToString());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Email, Name, PasswordHash, CreatedAt, UpdatedAt FROM Users WHERE Email = $email;";
            command.Parameters.AddWithValue("$email", email.ToString());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;

            return Map(reader);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Email, Name, PasswordHash, CreatedAt, UpdatedAt FROM Users WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id.ToString());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;

            return Map(reader);
        }


        private static User Map(SqliteDataReader reader)
        {
            return User.Restore(
                id: Guid.Parse(reader.GetString(0)),
                email: Email.Create(reader.GetString(1)),
                name: reader.GetString(2),
                passwordHash: reader.GetString(3),
                createdAt: DateTime.Parse(reader.GetString(4)),
                updatedAt: reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            );
        }
    }
}
