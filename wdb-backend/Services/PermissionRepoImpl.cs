using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class PermissionRepoImpl : IPermissionRepository
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

    public async Task<Permission> UpdateAsync(Guid permissionId, Permission permission, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken) ?? throw new KeyNotFoundException();
        item.Status = permission.Status;
        item.LastUpdatedAt = permission.LastUpdatedAt;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public Task<LinkedList<Permission>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Permission> GetOneAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken) ?? throw new KeyNotFoundException();
        return result;
    }

    public async Task<List<Permission>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Permissions.Where(x => x.WorkerId == workerId).ToListAsync(cancellationToken) ?? throw new KeyNotFoundException();
        return result;
    }
}