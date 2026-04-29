namespace wdb_backend.Abstractions;

public interface IUser
{
    Guid Id { get; set; }

    string Name { get; set; }

    string Email { get; set; }

    string Password { get; set; }

    DateTime CreatedAt { get; set; }

    bool Verified { get; set; }
}
