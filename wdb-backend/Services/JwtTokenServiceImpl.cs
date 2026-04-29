using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using wdb_backend.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace wdb_backend.Services;

public class JwtTokenServiceImpl : IJwtTokenService
{

    // inject JwtOptions from framework
    private readonly JwtOptions _options;

    public JwtTokenServiceImpl(IOptions<JwtOptions> options)
    {
        this._options = options.Value;
    }

    public string GenerateAccessToken(IUser user)
    {
        // construct token
        // get the key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        // get the credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // build the claims
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email),
            new ("username", user.Name),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // build token
        var token = new JwtSecurityToken(
            issuer:_options.Issuer,
            audience:_options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials:credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
