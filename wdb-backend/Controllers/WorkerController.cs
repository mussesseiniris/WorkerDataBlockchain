using System.Runtime.InteropServices.JavaScript;
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

    private readonly IWorkerInfoService _workerInfoService;

    public WorkerController(IPermissionService permissionService, IRequestService requestService, IWorkerInfoService workerInfoService)
    {
        _permissionService = permissionService;
        _requestService = requestService;
        _workerInfoService = workerInfoService;
    }

    [HttpGet("{workerId}/permissions")]
    public async Task<ActionResult<List<Permission>>> GetPermissions(Guid workerId)
    {
        var result = await _permissionService.GetAllByWorkerIdAsync(workerId, 0);

        if (result == null)
        {
            return NotFound(new { error = "WORKER_NOT_FOUND" });
        }

        return Ok(result);
    }

    [HttpGet("{workerId}/requests")]
    public async Task<ActionResult<Request>> GetRequestReason(Guid requestId)
    {
        var result = await _requestService.GetByRequestIdAsync(requestId);

        if (result == null)
        {
            return NotFound(new { error = "REQUEST_NOT_FOUND" });
        }

        return Ok(result);
    }

    [HttpGet("{workerId}/info")]
    public async Task<ActionResult<Request>> GetAllWorkerInfo(Guid workerId)
    {
        var result = await _workerInfoService.GetAllAsync(workerId);
        if (result == null)
        {
            return NotFound(new { error = "wORKER_NOT_FOUND" });
        }

        return Ok(result);

    }


    public class FieldResponse
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool Checked { get; set; } = false;
    }

    public class RequestRowResponse
    {
        public string Id { get; set; }
        public string Company { get; set; }
        public string Date { get; set; }
        public List<FieldResponse> Fields { get; set; }
        public string Reason { get; set; }
    }

    [HttpGet("{workerId}/rows")]
    public async Task<ActionResult> GetRows(Guid workerId)
    {
        var requests = await _requestService.GetAllByWorkerIdAsync(workerId);
        var permissions = await _permissionService.GetAllByWorkerIdAsync(workerId);
        var workerInfo = await _workerInfoService.GetAllAsync(workerId);

        var rows = requests.Select(req =>
        {
            var reqPermissions = permissions.Where(p => p.RequestId == req.Id).ToList();

            var workerInfos = reqPermissions.Select(p =>
            {
                var info = workerInfo.FirstOrDefault(w => w.Id == p.InfoId);
                return new FieldResponse
                {
                    Id = p.Id.ToString(),
                    Label = info.Desc,
                    Checked = false
                };
            }).ToList();
            return new RequestRowResponse
            {
                Id = req.Id.ToString(),
                Company = req.EmployerId.ToString(),
                Date = req.CreatedAt.ToString("dd.MM.yyyy hh:mm tt"),
                Fields = workerInfos,
                Reason = req.Reason
            };
        }).ToList();

        return Ok(rows);

    }


}
