using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;

namespace wdb_backend.Services;

public class WorkerDashboardServiceImpl : IWorkerDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<WorkerDashboardServiceImpl> _logger;

    public WorkerDashboardServiceImpl(
        AppDbContext dbContext,
        IBlockchainService blockchainService,
        ILogger<WorkerDashboardServiceImpl> logger)
    {
        _dbContext = dbContext;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<object?> GetDashboardAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
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
        )
        .Take(5)
        .ToListAsync(cancellationToken);

        var blockchainRecords = new List<object>();
        var blockchainAvailable = true;

        if (!string.IsNullOrWhiteSpace(worker.BlockchainAddress))
        {
            try
            {
                var logs = await _blockchainService.GetWorkerLogsAsync(
                    worker.BlockchainAddress,
                    cancellationToken
                );

                blockchainRecords = logs
                    .Take(5)
                    .Select(log => new
                    {
                        employerAddress = log.EmployerAddress,
                        workerAddress = log.WorkerAddress,
                        action = log.Action,
                        txHash = log.TxHash,
                        date = log.Date
                    })
                    .Cast<object>()
                    .ToList();
            }
            catch (Exception ex)
            {
                blockchainAvailable = false;

                _logger.LogWarning(
                    ex,
                    "Failed to load blockchain records for worker {WorkerId}",
                    workerId
                );
            }
        }

        return new
        {
            worker = new
            {
                id = worker.Id,
                name = worker.Name,
                email = worker.Email,
                verified = worker.Verified,
                blockchainAddress = worker.BlockchainAddress
            },
            latestRequests,
            blockchainRecords,
            blockchainAvailable
        };
    }
}
