using Microsoft.AspNetCore.Identity;
using wdb_backend.Abstractions;

namespace wdb_backend.Services;

public class PasswordHasherServiceImpl : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    // generate the hash for password
    public string HashPassword(string password)
    {
        return _hasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hashedPassword, password);
        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
