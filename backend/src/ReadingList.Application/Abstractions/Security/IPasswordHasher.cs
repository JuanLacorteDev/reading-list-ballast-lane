namespace ReadingList.Application.Abstractions.Security;

/// <summary>
/// Hashes and verifies passwords. The algorithm (e.g. PBKDF2) is an Infrastructure detail.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
