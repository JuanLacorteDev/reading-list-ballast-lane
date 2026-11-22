using ReadingList.Domain.Common.Base;
using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.Common.Validations;
using ReadingList.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace ReadingList.Domain.Entities
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public string PasswordHash { get; private set; }
        public Email Email { get; private set; }


        private User(Email email, string name, string passwordHash)
        {
            SetDatails(email, name, passwordHash);
        }

        private User(Guid id, Email email, string name, string passwordHash, DateTime createdAt, DateTime? updatedAt) : base(id, createdAt, updatedAt)
        {
            SetDatails(email, name, passwordHash);
        }

        public static User Create(Email email, string name, string passwordHash)
        {
            return new User(email, name, passwordHash);
        }

        public static User Restore(Guid id, Email email, string name, string passwordHash, DateTime createdAt, DateTime? updatedAt)
        {
            return new User(id, email, name, passwordHash, createdAt, updatedAt);
        }

        [MemberNotNull(nameof(Name), nameof(Email), nameof(PasswordHash))]
        private void SetDatails(Email email, string name, string passwordHash)
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            name = name.Trim();
            Guard.Against<DomainException>((name.Length > 50), $"Name must have at most 50 characters.");

            Guard.AgainstNull(email, nameof(email));
            Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));

            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }
    }
}
