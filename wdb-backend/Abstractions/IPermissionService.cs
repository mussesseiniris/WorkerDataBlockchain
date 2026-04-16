using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IPermissionService
{
    Task CreateAllByRequestAsync(Request request, IEnumerable<WorkerInfo> workerInfos, CancellationToken cancellationToken = default);

    Task<Permission> UpdateAsync(Guid requestId, Permission permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<Permission> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, int Status = -1, CancellationToken cancellationToken = default);
}
