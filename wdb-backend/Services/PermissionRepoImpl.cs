using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionRepoImpl:IPermissionRepository
{
    public Task AddAllByRequestAsync(Request request, LinkedList<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Permission> UpdateAsync(Guid requestId, Permission permission, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Permission> GetOneAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
