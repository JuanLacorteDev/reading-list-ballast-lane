using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ReadingList.Application.Abstractions.Security;
using ReadingList.Domain.Entities;
using System.Security.Claims;
using System.Text;

namespace ReadingList.Infrastructure.Security
{
    /// <summary>
    /// Issues HMAC-SHA256 signed JWT access tokens.
    /// </summary>
    public sealed class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        public AccessToken CreateToken(User user)
        {
            DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
            DateTime expiresAt = now.AddMinutes(_options.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email.ToString()),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = expiresAt,
                SigningCredentials = credentials
            };

            string token = _tokenHandler.CreateToken(descriptor);

            return new AccessToken(token, expiresAt);
        }
    }
}
