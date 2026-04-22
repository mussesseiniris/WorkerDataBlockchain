using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IPermissionRepository
{
    // add all permissions according to request
    Task AddAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default);

    // update status
    Task<Permission> UpdateAsync(Guid requestId, Permission permission, CancellationToken cancellationToken = default);

    // get all permissions of specific request id
    Task<LinkedList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    // get one permission by id
    Task<Permission> GetOneAsync(Guid permissionId, CancellationToken cancellationToken = default);

    // get all permissions by worker id
    Task<LinkedList<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    // add one permission according to request
    Task AddOneByRequestAsync(Request request, WorkerInfo workerInfo, CancellationToken cancellationToken=default);

}
