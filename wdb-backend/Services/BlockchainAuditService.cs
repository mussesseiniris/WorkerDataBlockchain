using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Data;

namespace wdb_backend.Services;

/// <summary>
/// Legacy lightweight blockchain audit service.
/// 
/// Important:
/// PermissionRequested should NOT be logged here anymore, because it needs
/// requestId, permissionIds and itemLabels. It is now logged as a request-level
/// record in CreateDataAccessRequestUsecaseImpl.
/// </summary>
public class BlockchainAuditService : IBlockchainAuditService
{
    private readonly AppDbContext _context;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<BlockchainAuditService> _logger;

    public BlockchainAuditService(
        AppDbContext context,
        IBlockchainService blockchainService,
        ILogger<BlockchainAuditService> logger)
    {
        _context = context;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task TryLogAsync(
        Guid employerId,
        Guid workerId,
        BlockchainAction action,
        CancellationToken ct = default)
    {
        if (action == BlockchainAction.PermissionRequested)
        {
            _logger.LogInformation(
                "Skip legacy PermissionRequested blockchain log. Request-level logging is handled by CreateDataAccessRequestUsecaseImpl. EmployerId={EmployerId}, WorkerId={WorkerId}",
                employerId,
                workerId);

            return;
        }

        try
        {
            var employer = await _context.Employers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employerId, ct);

            var worker = await _context.Workers
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workerId, ct);

            if (employer == null || worker == null)
            {
                _logger.LogWarning(
                    "Skip legacy blockchain log because employer or worker was not found. EmployerId={EmployerId}, WorkerId={WorkerId}, Action={Action}",
                    employerId,
                    workerId,
                    action);
                return;
            }

            if (string.IsNullOrWhiteSpace(employer.PrivateKey) ||
                string.IsNullOrWhiteSpace(employer.BlockchainAddress) ||
                string.IsNullOrWhiteSpace(worker.BlockchainAddress))
            {
                _logger.LogWarning(
                    "Skip legacy blockchain log because blockchain keys or addresses are missing. EmployerId={EmployerId}, WorkerId={WorkerId}, Action={Action}",
                    employerId,
                    workerId,
                    action);
                return;
            }

            await _blockchainService.LogCategoryTransactionAsync(
                privateKey: employer.PrivateKey!,
                employerAddress: employer.BlockchainAddress!,
                workerAddress: worker.BlockchainAddress!,
                requestId: string.Empty,
                category: "Unknown",
                permissionIds: string.Empty,
                itemLabels: string.Empty,
                action: action,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to log {Action} on-chain using legacy BlockchainAuditService.",
                action);
        }
    }
}
