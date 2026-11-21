using ReadingList.Application.Abstractions.Security;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Tests.Fakes
{
    public sealed class FakeTokenService : ITokenService
    {
        public static readonly DateTime FixedExpiresAt = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public User? LastUser { get; private set; }  

        public AccessToken CreateToken(User user)
        {
            LastUser = user;
            return new AccessToken($"token-for-{user.Id}", FixedExpiresAt);
        }
    }
}


