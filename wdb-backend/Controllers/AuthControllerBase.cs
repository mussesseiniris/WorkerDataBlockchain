using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Models;
using wdb_backend.Services;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;
[ApiController]
public abstract class AuthControllerBase<T> : ControllerBase where T : class, IUser, new()
{
    private readonly AuthService<T> _authService;

    // constructor
    public AuthControllerBase(AuthService<T> authService)
    {
        this._authService = authService;
    }

    // handle the register request
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest registerRequest, CancellationToken ct = default)
    {
        var (success, message) = await _authService.RegisterAsync(registerRequest, ct);
        if (!success)
        {
            return BadRequest(ApiResponse<object>.Fail(message));
        }
        return Ok(ApiResponse<object>.Ok(message));
    }


    // handle the login request
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResult>>> login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        var (success, message, result) = await _authService.LoginAsync(request, ct);
        if (! success || result is null)
        {
            return Unauthorized(ApiResponse<AuthResult>.Fail(message));
        }

        return Ok(ApiResponse<AuthResult>.Ok(result, message));
    }

}

[Route("api/workers")]
public class WorkerAuthController : AuthControllerBase<Worker>
{
    public WorkerAuthController(AuthService<Worker> authService) : base(authService) {}
}

[Route("api/employers")]
public class EmployerAuthController : AuthControllerBase<Employer>{
    public EmployerAuthController(AuthService<Employer> authService) : base(authService) {}
}
