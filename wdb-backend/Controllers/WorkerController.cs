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
public class WorkerController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly IRequestService _requestService;

    public WorkerController(IPermissionService permissionService, IRequestService requestService)
    {
        _permissionService = permissionService;
        _requestService = requestService;
    }

    [HttpGet("{workerId}/permissions")]
    public async Task<ActionResult<List<Permission>>> GetPermissions(Guid workerId, CancellationToken cancellationToken)
    {
        var result = await _permissionService.GetAllByWorkerIdAsync(workerId, 0, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND" });
        }

        return Ok(result);
    }

    [HttpGet("{workerId}/requests")]
    public async Task<ActionResult<Request>> GetRequestReason(Guid workerId, Guid requestId)
    {
        var result = await _requestService.GetByRequestIdAsync(workerId, requestId);

        if (result == null)
        {
            return NotFound(new { error = "REQUEST_NOT_FOUND" });
        }

        return Ok(result);
    }

}
