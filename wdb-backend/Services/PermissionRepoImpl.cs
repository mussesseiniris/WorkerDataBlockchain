using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionRepoImpl : IPermissionRepository
{
    private readonly AppDbContext _context;
    public PermissionRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAllByRequestAsync(Request request, List<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
        foreach (var workerInfo in workerInfos)
        {
            await AddOneByRequestAsync(request, workerInfo, cancellationToken);
        }

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

    public async Task AddOneByRequestAsync(Request request, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        var permission = new Permission { InfoId = workerInfo.Id, RequestId = request.Id, WorkerId = workerInfo.WorkerId, Status = Common.PermissionStatus.Pending };
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);
    }

}
