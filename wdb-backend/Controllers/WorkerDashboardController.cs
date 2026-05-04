using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for worker dashboard operations.
/// </summary>

[ApiController]
[Route("api/worker/dashboard")]
public class WorkerDashboardController : ControllerBase
{
    private readonly IWorkerDashboardService _workerDashboardService;

    public WorkerDashboardController(IWorkerDashboardService workerDashboardService)
    {
        _workerDashboardService = workerDashboardService;
    }

    /// <summary>
    /// Returns dashboard data for the given worker.
    /// </summary>
    /// <param name="workerId">The worker ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// 200 OK with dashboard data if worker exists;
    /// 404 Not Found if worker does not exist.
    /// </returns>
    [HttpGet("{workerId:guid}")]
    public async Task<IActionResult> GetDashboard(Guid workerId, CancellationToken cancellationToken)
    {
        var result = await _workerDashboardService.GetDashboardAsync(workerId, cancellationToken);

        if (result == null)
        {
            return NotFound(new { error = "Worker not found" });
        }

        return Ok(result);
    }
}
