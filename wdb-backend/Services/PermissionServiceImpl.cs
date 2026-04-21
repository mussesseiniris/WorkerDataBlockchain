using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionServiceImpl:IPermissionService
{
    private readonly IPermissionRepository _permissionRepo;
    public PermissionServiceImpl(IPermissionRepository permissionRepo)
    {
        _permissionRepo = permissionRepo;
        
    }
    public async Task CreateAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
         await _permissionRepo.AddAllByRequestAsync(request,workerInfos,cancellationToken);
    }

    public Task<Permission> UpdateAsync(Guid requestId, Permission permission, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Permission> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CreateAllByRequestAsync(Request request, HashSet<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
