using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Net.Http.Headers;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionServiceImpl : IPermissionService
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

    /// <summary>
    /// Updates permission status and timestamp for given permission ID
    /// </summary>
    /// <param name="permissionId">The ID of given permission to update</param>
    /// <param name="status">
    /// Changes permission status:
    /// 0 = Pending, 1 = Approve, 2 = Reject
    /// </param>
    /// <param name="cancellationToken">Token to cancel async operation</param>
    /// <returns>Returns permission item with updated status and timestamp</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<Permission> UpdateAsync(Guid permissionId, int status, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetOneAsync(permissionId, cancellationToken) ?? throw new KeyNotFoundException();
        if (permission.Status != PermissionStatus.Pending)
        {
            throw new InvalidOperationException($"Permission {permissionId} cannot be udpated as it is no longer pending");
        }

        permission.Status = (PermissionStatus)status;
        permission.LastUpdatedAt = DateTime.UtcNow;
        var result = await _permissionRepository.UpdateAsync(permissionId, permission, cancellationToken);
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
        var result = await _permissionRepository.GetAllByWorkerIdAsync(workerId, cancellationToken) ?? throw new KeyNotFoundException();
        if (Status != -1)
        {
            result = result.Where(x => x.Status == (PermissionStatus)Status).ToList();
        }

        return result;
    }
}
