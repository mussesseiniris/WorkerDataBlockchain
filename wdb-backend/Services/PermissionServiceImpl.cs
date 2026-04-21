using Microsoft.Net.Http.Headers;
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

    public async Task<Permission> UpdateAsync(Guid permissionId, int status = 0, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetOneAsync(permissionId)??throw new KeyNotFoundException();
        permission.Status = (PermissionStatus)status;
        permission.LastUpdatedAt = DateTime.UtcNow;
        var result = await _permissionRepository.UpdateAsync(permissionId, permission)??throw new KeyNotFoundException();
        return result;
    }

    public Task<IReadOnlyList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Permission> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves permissions for given worker. Optionally filters by permission status.
    /// </summary>
    /// <param name="workerId">The ID of wokrer whose permissions to retrieve </param>
    /// <param name="Status"> 
    /// Optional. Filters results by permission status:
    /// 0 = Pending, 1 = Approved, 2 = Rejected.
    /// If not provided, all permission regardless of status are returned.
    /// </param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>A list of permissions</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no permissions are found for given worker ID </exception>
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
