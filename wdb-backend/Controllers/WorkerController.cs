using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using System.Security.Claims;

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
    private readonly IEmployerService _employerService;

    public WorkerController(IPermissionService permissionService, IRequestService requestService, IWorkerInfoService workerInfoService, IEmployerService employerService)
    {
        _permissionService = permissionService;
        _requestService = requestService;
        _workerInfoService = workerInfoService;
        _employerService = employerService;
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
        public required string Id { get; set; }
        public required string Label { get; set; }
        public required bool Checked { get; set; } = false;
    }

    public class RequestRowResponse
    {
        public required string Id { get; set; }
        public required string Company { get; set; }
        public required string Date { get; set; }
        public required List<FieldResponse> Fields { get; set; }
        public required string Reason { get; set; }
    }

    [Authorize]
    [HttpGet("rows")]
    public async Task<ActionResult> GetRows()
    {

        var workerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (workerId == null) return Unauthorized();
        var workerGuid = Guid.Parse(workerId);

        var requests = await _requestService.GetAllByWorkerIdAsync(workerGuid);
        var workerInfo = await _workerInfoService.GetAllAsync(workerGuid);
        var permissions = await _permissionService.GetAllByWorkerIdAsync(workerGuid, 0);
        var groupedPermissions = permissions.GroupBy(p => p.RequestId);

        var employers = await _employerService.GetDistinctEmployers();
        var employerMap = employers.ToDictionary(e => e.Id);

        var rows = new List<RequestRowResponse>();

        foreach (var group in groupedPermissions)
        {
            var request = requests.FirstOrDefault(p => p.Id == group.Key);
            if (request == null) continue;

            employerMap.TryGetValue(request.EmployerId, out var employer);

            var workerInfos = new List<FieldResponse>();
            foreach (var p in group)
            {
                var info = workerInfo.FirstOrDefault(w => w.Id == p.InfoId);
                workerInfos.Add(new FieldResponse
                {
                    Id = p.Id.ToString(),
                    Label = info?.Desc ?? "Unknown",
                    Checked = false
                });
            }
            rows.Add(new RequestRowResponse
            {
                Id = request.Id.ToString(),
                Company = employer?.Name.ToString() ?? "Unknown",
                Date = request.CreatedAt.ToString("dd.MM.yyyy hh:mm tt"),
                Fields = workerInfos,
                Reason = request.Reason
            });

        }
        ;
        return Ok(rows);

    }


    public class ActiveAccessRow
    {
        public required string Id { get; set; }
        public required string Company { get; set; }
        public required string Date { get; set; }
        public required List<FieldResponse> WorkerInfo { get; set; }
        public required string Reason { get; set; }
    }

    [HttpGet("{workerId}/active-access")]
    public async Task<ActionResult> GetActiveAccess(Guid workerId)
    {
        var requests = await _requestService.GetAllByWorkerIdAsync(workerId);
        var permissions = await _permissionService.GetAllByWorkerIdAsync(workerId, 1);
        var workerInfo = await _workerInfoService.GetAllAsync(workerId);

        var rows = new List<ActiveAccessRow>();

        foreach (var req in requests)
        {
            var approvedPermissions = permissions.Where(p => p.RequestId == req.Id).ToList();
            if (!approvedPermissions.Any()) continue;

            var fields = new List<FieldResponse>();
            foreach (var p in approvedPermissions)
            {
                var info = workerInfo.FirstOrDefault(w => w.Id == p.InfoId);
                fields.Add(new FieldResponse
                {
                    Id = p.Id.ToString(),
                    Label = info?.Desc ?? "Unknown",
                    Checked = false
                });
            }

            var employer = await _employerService.GetEmployerInfoAsync(req.EmployerId);
            rows.Add(new ActiveAccessRow
            {
                Id = req.Id.ToString(),
                Company = employer?.Name.ToString() ?? "Unknown",
                Date = req.CreatedAt.ToString("dd.MM.yyyy hh:mm tt"),
                WorkerInfo = fields,
                Reason = req.Reason
            });
        }

        return Ok(rows);
    }

}



