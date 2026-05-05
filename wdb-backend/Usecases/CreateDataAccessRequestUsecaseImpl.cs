using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;
namespace wdb_backend.Usecases;

/// <summary>
/// Orchestrates RequestService and PermissionService to create a data access request.
/// </summary>
public class CreateDataAccessRequestUsecaseImpl : ICreateDataAccessRequestUsecase
{
    private readonly IPermissionService _permService;
    private readonly IRequestService _requService;
    public CreateDataAccessRequestUsecaseImpl(IRequestService requService, IPermissionService permService)
    {
        _requService = requService;
        _permService = permService;
    }
    public async Task CreateDataAccessRequest(List<WorkerInfo> workerInfos, Guid employerId, Guid workerId, string reason)
    {
        var request = await _requService.CreateAsync(employerId, workerId, reason);
        await _permService.CreateAllByRequestAsync(request, workerInfos);
    }

}
