using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using RegisterRequest = wdb_backend.DTOs.RegisterRequest;

namespace wdb_backend.Services;

public class AuthService<T> where T : class, IUser, new()
{
    // inject necessary instances
    private readonly IUserRepository<T> _userRepo;

    // used for generate hash and verify password
    private readonly IPasswordHasher _hasher;

    // need jwt service to generate token used for id
    private readonly IJwtTokenService _jwtservice;

    public AuthService(IUserRepository<T> userRepo, IPasswordHasher hasher, IJwtTokenService jwtService)
    {
        _userRepo = userRepo;
        _hasher = hasher;
        _jwtservice = jwtService;
    }

    // handle register logic
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        // get register info
        string email = request.Email.Trim().ToLowerInvariant();
        string userName = request.UserName.Trim();
        // verify the register info
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Invalid Input.");
        }

        // check whether the user in the system
        bool existsFlag = await _userRepo.EmailExistsAsync(email, ct);
        if (existsFlag) return (false, "Email already exists.");

        // create user and then save to database, note: the password should be hashed to be stored into database
        T user = new T
        {
            Name = userName,
            Email = email,
            Password = _hasher.HashPassword(request.Password),
            Verified = false
        };

        // save user into database
        await _userRepo.AddAsync(user, ct);

        // there is no issue of the register info
        return (true, string.Empty);
    }


    // handle login logic
    public async Task<(bool Success, string Message, AuthResult result)> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        // get the login info from LoginRequest
        string email = request.Email.Trim().ToLowerInvariant();
        string password = request.Password.Trim();

        // verify if the input info is correct or not
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Invalid email or password.", null);
        }

        // check whether the user is in the database, query user by email
        IUser? user = await _userRepo.GetByEmailAsync(email, ct);

        // check the email and password
        if (user == null)
        {
            return (false, "User not found.", null);
        }

        if (! _hasher.VerifyPassword(password, user.Password))
        {
            return (false, "Incorrect password.", null);
        }

        // get the token
        string token = _jwtservice.GenerateAccessToken(user);
        AuthResult result = new AuthResult(token, user.Name, user.Email, user.Id);

        return (true, "Login Successful.", result);
    }

}
