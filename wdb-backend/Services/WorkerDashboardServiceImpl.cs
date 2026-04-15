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

    public Task<object?> GetDashboardAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object?>(null);
    }
}
