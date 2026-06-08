using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wdb_backend.Abstractions;

namespace wdb_backend.Controllers;

/// <summary>
/// Certification API for employers.
/// Route: api/certification
/// </summary>
[Authorize]
[ApiController]
[Route("api/certification")]
public class CertificationController : ControllerBase
{
    private readonly ICertificationService _certificationService;

    public CertificationController(ICertificationService certificationService)
    {
        _certificationService = certificationService;
    }


    private Guid GetCurrentEmployerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(claim.Value);
    }

    // ── POST /api/certification/upload ────────────────────────────────────

    /// <summary>
    /// Upload a certification document.
    /// Only allowed when status is null or Rejected.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();
            var result = await _certificationService.UploadCertificationAsync(employerId, file, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ── GET /api/certification/status ─────────────────────────────────────

    /// <summary>
    /// Get current certification status for the logged-in employer.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        try
        {
            var employerId = GetCurrentEmployerId();
            var result = await _certificationService.GetCertificationStatusAsync(employerId, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}