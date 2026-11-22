using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ReadingList.Infrastructure.Persistence
{
    public interface IDbConnectionFactory
    {
        SqliteConnection Create();
    }
    public sealed class SqliteConnectionFactory : IDbConnectionFactory
    {
        private string _connectionString;
        public SqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteConnection Create()
        {
            return new(_connectionString);
        }
    }
}
