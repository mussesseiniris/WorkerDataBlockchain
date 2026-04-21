using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionRepoImpl:IPermissionRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task AddAllByRequestAsync(Request request, LinkedList<WorkerInfo> workerInfos, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Permission> UpdateAsync(Guid permissionId, Permission permission, CancellationToken cancellationToken = default)
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

    public async Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Permissions.Where(x => x.WorkerId == workerId).ToListAsync(cancellationToken);
        return result;
    }
}
