using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;

namespace wdb_backend.Services;

public class WorkerDashboardServiceImpl : IWorkerDashboardService
{
    private readonly AppDbContext _dbContext;

    public WorkerDashboardServiceImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<object?> GetDashboardAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var worker = await _dbContext.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == workerId, cancellationToken);

        if (worker == null)
        {
            return null;
        }

        return new
        {
            worker = new
            {
                id = worker.Id,
                name = worker.Name,
                email = worker.Email,
                verified = worker.Verified
            },
            latestPermissions = new List<object>(),
            blockchainRecords = new List<object>()
        };
    }
}
