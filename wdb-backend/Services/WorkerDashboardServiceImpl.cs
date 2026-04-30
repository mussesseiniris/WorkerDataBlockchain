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

        var latestRequests = await (
            from r in _dbContext.Requests.AsNoTracking()
            join e in _dbContext.Employers.AsNoTracking()
                on r.EmployerId equals e.Id
            where r.WorkerId == workerId
            orderby r.CreatedAt descending
            select new
            {
                requestId = r.Id,
                employerId = r.EmployerId,
                employerName = e.Name,
                createdAt = r.CreatedAt,
                reason = r.Reason
            }
        ).Take(5).ToListAsync(cancellationToken);

        return new
        {
            worker = new
            {
                id = worker.Id,
                name = worker.Name,
                email = worker.Email,
                verified = worker.Verified
            },
            latestRequests = latestRequests,
            blockchainRecords = new List<object>()
        };
    }
}
