using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/employers")]
public class EmployerDashboardController : ControllerBase
{
    private readonly IEmployerDashboardService _employerDashboardService;

    public EmployerDashboardController(
        IEmployerDashboardService employerDashboardService
    )
    {
        _employerDashboardService = employerDashboardService;
    }

    [Authorize]
    [HttpGet("me/dashboard")]
    public async Task<IActionResult> GetMyDashboard(
        CancellationToken cancellationToken
    )
    {
        var employerId = GetCurrentUserId();

        if (employerId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Missing or invalid user id in token."
            });
        }

        try
        {
            var dashboard = await _employerDashboardService.GetDashboardAsync(
                employerId.Value,
                cancellationToken
            );

            return Ok(new
            {
                success = true,
                data = dashboard,
                message = "Employer dashboard data retrieved."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
