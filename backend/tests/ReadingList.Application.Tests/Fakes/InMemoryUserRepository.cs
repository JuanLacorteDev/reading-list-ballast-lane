using ReadingList.Application.Abstractions.Persistence;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ReadingList.Application.Tests.Fakes
{
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];

        public IReadOnlyList<User> Users => _users;
        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Any(u => u.Email.Equals(email)));
        }

        public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email)));
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public Task<bool> LoginAsync(Email email, string passwordHash, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Any(u => u.Email.Equals(email) && u.PasswordHash.Equals(passwordHash)));
        }
    }
}
