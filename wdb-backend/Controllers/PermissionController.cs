using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Services;
namespace wdb_backend.Controllers;

/// <summary>
/// API controller for managing worker-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("{permissionid}/approve")]
    public async Task<ActionResult<Permission>> ApprovePermission(Guid permissionId, CancellationToken cancellationToken)
    {
        try
        {
            var update = await _permissionService.UpdateAsync(permissionId, 1, cancellationToken);
            return Ok(update);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "PERMISSION_NOT_FOUND" });
        }
        catch (InvalidOperationException)
        {
            return UnprocessableEntity(new { error = "INAVLID_STATUS_CHANGE" });
        }

    }

    [HttpGet("{permissionid}/reject")]
    public async Task<ActionResult<Permission>> RejectPermission(Guid permissionId, CancellationToken cancellationToken)
    {
        try
        {
            var update = await _permissionService.UpdateAsync(permissionId, 2, cancellationToken);
            return Ok(update);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "PERMISSION_NOT_FOUND" });
        }
        catch (InvalidOperationException)
        {
            return UnprocessableEntity(new { error = "INAVLID_STATUS_CHANGE" });
        }

    }

}