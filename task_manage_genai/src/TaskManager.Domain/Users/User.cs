using TaskManager.Domain.Common;

namespace TaskManager.Domain.Users;

public sealed class User
{
    private User(Guid id, string name)
    {
        if (id == Guid.Empty)
            throw new DomainException("User.IdRequired", "User id must not be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("User.NameRequired", "Name is required and must not be blank.");

        Id = id;
        Name = name.Trim();
    }

    public Guid Id { get; }
    public string Name { get; }

    public static User Create(string name) => new(Guid.NewGuid(), name);

    /// <summary>Rebuilds an existing user from persisted state.</summary>
    public static User Restore(Guid id, string name) => new(id, name);
}
