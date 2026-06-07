using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Dtos;

namespace wdb_backend.Services;

public class WorkerAuditLogServiceImpl : IWorkerAuditLogService
{
    private readonly AppDbContext _dbContext;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<WorkerAuditLogServiceImpl> _logger;

    public WorkerAuditLogServiceImpl(
        AppDbContext dbContext,
        IBlockchainService blockchainService,
        ILogger<WorkerAuditLogServiceImpl> logger)
    {
        _dbContext = dbContext;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<WorkerAuditLogResponseDto> GetWorkerAuditLogAsync(Guid workerId)
    {
        var worker = await _dbContext.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workerId);

        if (worker == null)
            throw new KeyNotFoundException("Worker not found.");

        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
            return EmptyResponse(workerId);

        List<Models.BlockchainTransactionResponse> blockchainRecords;

        try
        {
            blockchainRecords = await _blockchainService.GetWorkerLogsAsync(worker.BlockchainAddress);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Blockchain audit log is unavailable for worker {WorkerId}. Returning empty audit log.",
                workerId);

            return EmptyResponse(workerId);
        }

        var employerAddresses = blockchainRecords
            .Select(record => record.EmployerAddress)
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var employers = await _dbContext.Employers
            .AsNoTracking()
            .Where(employer =>
                employer.BlockchainAddress != null &&
                employerAddresses.Contains(employer.BlockchainAddress))
            .ToListAsync();

        var employerNameByAddress = employers
            .Where(employer => !string.IsNullOrWhiteSpace(employer.BlockchainAddress))
            .GroupBy(employer => employer.BlockchainAddress!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First().Name,
                StringComparer.OrdinalIgnoreCase);

        var records = blockchainRecords
            .Select(record =>
            {
                var employerName = employerNameByAddress.TryGetValue(
                    record.EmployerAddress,
                    out var matchedEmployerName)
                    ? matchedEmployerName
                    : "Unknown company";

                var categoryLabel = ToCategoryLabel(record.Category);
                var itemLabels = UsesStructuredSummary(record)
                    ? SplitStructuredSummary(record.ItemLabels)
                    : SplitCsv(record.ItemLabels);

                return new AuditLogRecordDto
                {
                    Action = record.Action,
                    ActionLabel = GetActionLabel(record.Action),
                    UserMessage = GetUserMessage(record.Action, employerName, categoryLabel),
                    EmployerName = employerName,

                    RequestId = record.RequestId,
                    Category = record.Category,
                    CategoryLabel = categoryLabel,
                    PermissionIds = record.PermissionIds,
                    ItemLabels = itemLabels,

                    EmployerAddress = record.EmployerAddress,
                    WorkerAddress = record.WorkerAddress,
                    TransactionHash = record.TxHash,
                    BlockHash = record.BlockHash,
                    CreatedAt = record.Date
                };
            })
            .OrderByDescending(record => record.CreatedAt)
            .ToList();

        return new WorkerAuditLogResponseDto
        {
            WorkerId = workerId,
            Records = records
        };
    }

    private static WorkerAuditLogResponseDto EmptyResponse(Guid workerId)
    {
        return new WorkerAuditLogResponseDto
        {
            WorkerId = workerId,
            Records = new List<AuditLogRecordDto>()
        };
    }

    private static bool UsesStructuredSummary(Models.BlockchainTransactionResponse record)
    {
        return record.Action == "RequestReviewed"
            || record.Category == "RequestAccess"
            || record.ItemLabels.Contains('|');
    }

    private static List<string> SplitCsv(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> SplitStructuredSummary(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value
            .Split("||", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static string ToCategoryLabel(string category)
    {
        return category switch
        {
            "PersonalInformation" => "Personal Information",
            "MedicalInformation" => "Medical Information",
            "CareerInformation" => "Career Information",
            "FinancialInformation" => "Financial Information",
            "WorkplaceInformation" => "Workplace Information",
            "OtherInformation" => "Other Information",
            "RequestReview" => "Request Review",
            "RequestAccess" => "Access Grant",
            "" => "Information",
            null => "Information",
            _ => category
        };
    }

    private static string GetActionLabel(string action)
    {
        return action switch
        {
            "PermissionRequested" => "Access Requested",
            "PermissionApproved" => "Access Approved",
            "PermissionRejected" => "Request Rejected",
            "DataViewed" => "Data Viewed",
            "PermissionRevoked" => "Access Revoked",
            "RequestReviewed" => "Request Reviewed",
            _ => "Access Record"
        };
    }

    private static string GetUserMessage(
        string action,
        string employerName,
        string categoryLabel)
    {
        return action switch
        {
            "PermissionRequested" =>
                $"{employerName} requested access to your {categoryLabel}.",

            "PermissionApproved" =>
                $"You approved {employerName} to access your {categoryLabel}.",

            "PermissionRejected" =>
                $"You rejected {employerName}'s request for your {categoryLabel}.",

            "DataViewed" =>
                $"{employerName} viewed your {categoryLabel}.",

            "PermissionRevoked" =>
                $"You revoked {employerName}'s access.",

            "RequestReviewed" =>
                $"You reviewed {employerName}'s data access request.",

            _ =>
                $"An access-related action involving {employerName} and your {categoryLabel} was recorded."
        };
    }
}
