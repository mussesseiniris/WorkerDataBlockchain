namespace wdb_backend.Abstractions;

public interface IPasswordHasher
{
    // generate the hash password
    string HashPassword(string password);

    // verify the password
    bool VerifyPassword(string password, string hashedPassword);

}
