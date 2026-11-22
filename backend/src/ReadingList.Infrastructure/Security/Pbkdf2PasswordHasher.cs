using ReadingList.Application.Abstractions.Security;
using System.Security.Cryptography;

namespace ReadingList.Infrastructure.Security
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-10.0
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;          
        private const int HashSize = 32;          
        private const int Iterations = 100_000;   
        private const char Delimiter = '.';

        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: Iterations,
                hashAlgorithm: Algorithm,
                outputLength: HashSize);

            return string.Join(
                Delimiter,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public bool Verify(string password, string passwordHash)
        {
            if (password is null || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            string[] parts = passwordHash.Split(Delimiter);
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int iterations) || iterations is <= 0 or > Iterations)
                return false;

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length == 0 || expectedHash.Length == 0)
                return false;

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: iterations,
                hashAlgorithm: Algorithm,
                outputLength: expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
