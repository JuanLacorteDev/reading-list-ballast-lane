using Microsoft.Data.Sqlite;
using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace ReadingList.Infrastructure.Persistence.Books
{
    public sealed class BookRespository : IBookRepository
    {
        private readonly IDbConnectionFactory _factory;
        public BookRespository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO BOOKS (Id, OwnerId, Title, Author, Status, CreatedAt) 
                VALUES ($id, $ownerId, $title, $author, $status, $createdAt);
                """;

            command.Parameters.AddWithValue("$id", book.Id);
            command.Parameters.AddWithValue("$ownerId", book.OwnerId);
            command.Parameters.AddWithValue("$title", book.Title);
            command.Parameters.AddWithValue("$author", book.Author);
            command.Parameters.AddWithValue("$createdAt", book.CreatedAt.ToString("O", CultureInfo.InvariantCulture));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();

            command.CommandText = """
                DELETE FROM BOOKS WHERE Id = $id AND OwnerId = $ownerId;
                """;
            command.Parameters.AddWithValue("$id", id.ToString());
            command.Parameters.AddWithValue("$ownerId", ownerId.ToString());

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<Book?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT Id, OwnerId, Title, Author, Status, Rating, CreatedAt, UpdatedAt FROM BOOKS WHERE Id = $id AND OwnerId = $ownerId;
                """;
            command.Parameters.AddWithValue("$id", id.ToString());
            command.Parameters.AddWithValue("$ownerId", ownerId.ToString());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;

            return Map(reader);
        }

        public async Task<IReadOnlyList<Book>> ListByOwnerAsync(Guid ownerId, ReadingStatus? status = null, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();

            var sql = new StringBuilder("""
                SELECT Id, OwnerId, Title, Author, Status, Rating, CreatedAt, UpdatedAt
                FROM Books
                WHERE OwnerId = $ownerId
            """);

            command.Parameters.AddWithValue("$ownerId", ownerId.ToString());

            if (status is not null)
            {
                sql.Append(" AND Status = $status");
                command.Parameters.AddWithValue("$status", status.Value.ToString());
            }

            sql.Append(" ORDER BY CreatedAt DESC;");
            command.CommandText = sql.ToString();

            var books = new List<Book>();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                books.Add(Map(reader));
            }

            return books;

        }

        public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.Create();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE Books
                SET Title     = $title,
                    Author    = $author,
                    Status    = $status,
                    Rating    = $rating,
                    UpdatedAt = $updatedAt
                WHERE Id = $id AND OwnerId = $ownerId;
            """;

            command.Parameters.AddWithValue("$id", book.Id.ToString());
            command.Parameters.AddWithValue("$ownerId", book.OwnerId.ToString());
            command.Parameters.AddWithValue("$title", book.Title);
            command.Parameters.AddWithValue("$author", book.Author);
            command.Parameters.AddWithValue("$status", book.Status.ToString());
            command.Parameters.AddWithValue("$rating", (object?)book.Rating ?? DBNull.Value);
            command.Parameters.AddWithValue("$updatedAt", book.UpdatedAt?.ToString("O", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        private static Book Map(SqliteDataReader reader)
        {
            return Book.Restore(
               id: reader.GetGuid(0),
               ownerId: reader.GetGuid(1),
               title: reader.GetString(2),
               author: reader.GetString(3),
               status: (ReadingStatus)reader.GetInt32(4),
               rating: reader.IsDBNull(5) ? null : reader.GetInt32(5),
               createdAt: DateTime.Parse(reader.GetString(6)),
               updatedAt: reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
            );
        }
    }
}
