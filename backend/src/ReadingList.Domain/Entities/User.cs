using ReadingList.Domain.Common.Base;
using ReadingList.Domain.Common.Exceptions;
using ReadingList.Domain.Common.Validations;
using ReadingList.Domain.ValueObjects;
using System.Net.NetworkInformation;

namespace ReadingList.Domain.Entities
{
    public class User : Entity
    {
        public string Name { get; private set; }
        public string PasswordHash { get; private set;  }
        public Email Email { get; private set; }


        private User(Email email, string name,  string passwordHash)
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));

            Guard.AgainstNull(email, nameof(email));

            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }

        public static User Create(Email email, string name, string passwordHash)
        {
            return new User(email, name, passwordHash);
        }
    }
}
