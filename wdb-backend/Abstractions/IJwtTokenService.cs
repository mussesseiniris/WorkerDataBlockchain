using wdb_backend.Abstractions;

namespace wdb_backend.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(IUser user);
}
