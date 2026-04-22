using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployerController : ControllerBase
{
    private readonly ICreateDataAccessRequestUsecase _createDataAccessUsecase;
    private readonly IFindWorkerInfosByEmailUsecase _findWorkerInfosUsecase;

    public EmployerController(ICreateDataAccessRequestUsecase createDataAccessUsecase, IFindWorkerInfosByEmailUsecase findWorkerInfosUsecase)
    {
        _createDataAccessUsecase = createDataAccessUsecase;
        _findWorkerInfosUsecase = findWorkerInfosUsecase;

    }

    /// <summary>
    /// Get all of the worker's information by Email.
    /// </summary>
    /// <param name="email">The worker's email address.</param>
    /// <returns>200 OK with list of worker info, or 4404 Notfound if worker not found</returns>
    [HttpGet]
    public async Task<ActionResult<List<WorkerInfo>>> GetWorkerInfosByEmail(string email)
    {
        var workerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(email);
        if (workerInfos.Count == 0) { return NotFound(); }
        return Ok(workerInfos);
    }

    /// <summary>
    /// Creates a data access request for selected worker information.
    /// </summary>
    /// <param name="email">The worker's email address.</param>
    /// <param name="infoDesc">List of selected worker info descriptions.</param>
    /// <param name="reason">The reason for requesting access.</param>
    /// <param name="employerId">The ID of the employer making the request.</param>
    /// <returns>200 OK if successful, 404 if worker not found.</returns>
    [HttpPost("CreateRequest")]
    public async Task<ActionResult> CreateRequest(string email, List<string> infoDesc, string reason, Guid employerId)
    {
        var allWorkerInfos = await _findWorkerInfosUsecase.FindWorkerInfosByEmail(email);
        if (allWorkerInfos == null || allWorkerInfos.Count == 0)
        {
            return NotFound();
        }
        var selectedInfos = allWorkerInfos.Where(w => infoDesc.Contains(w.Desc, StringComparer.OrdinalIgnoreCase)).ToList();
        var worker_id = allWorkerInfos[0].WorkerId;
        await _createDataAccessUsecase.CreateDataAccessRequest(selectedInfos, employerId, worker_id, reason);
        return Ok();
    }

}
