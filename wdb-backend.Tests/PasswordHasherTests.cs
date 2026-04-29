using wdb_backend.Services;

namespace wdb_backend.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasherServiceImpl _hasher = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        var hash = _hasher.HashPassword("Password123");

        Assert.False(string.IsNullOrEmpty(hash));
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHash()
    {
        var hash = _hasher.HashPassword("Password123");

        Assert.NotEqual("Password123", hash);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.HashPassword("Password123");

        var result = _hasher.VerifyPassword("Password123", hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.HashPassword("Password123");

        var result = _hasher.VerifyPassword("Password123456", hash);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentHashes()
    {
        var hash1 = _hasher.HashPassword("Password123");
        var hash2 = _hasher.HashPassword("Password123");

        Assert.NotEqual(hash1, hash2);
    }
}
