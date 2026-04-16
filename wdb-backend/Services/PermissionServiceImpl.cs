using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionServiceImpl:IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    public PermissionServiceImpl(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }
    public Task CreateAllByRequestAsync(Request request, IEnumerable<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    public async Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, int Status = -1, CancellationToken cancellationToken = default)
    {
       var result = await _permissionRepository.GetAllByWorkerIdAsync(workerId)??throw new KeyNotFoundException();
       if (Status != -1)
        {
           result = result.Where(x => x.Status == (PermissionStatus)Status).ToList();
        }
       
       return result;
    }
}
