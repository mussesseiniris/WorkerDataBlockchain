using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IPermissionService
{
    Task CreateAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default);

    Task<Permission> UpdateAsync(Guid requestId, Permission permission, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<Permission> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);
}
