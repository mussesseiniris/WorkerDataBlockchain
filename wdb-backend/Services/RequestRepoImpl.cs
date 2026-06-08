using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestRepoImpl : IRequestRepository
{
    private readonly AppDbContext _dbContext;

    public RequestRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Create a new request.
    /// ExpiryDate is intentionally null because the worker sets it during review.
    /// If customRequest is provided, it is stored as a pending custom request.
    /// </summary>
    public async Task<Request> AddAsync(
        Guid employerId,
        Guid workerId,
        string reason,
        string? customRequest = null,
        CancellationToken cancellationToken = default)
    {
        var trimmedCustomRequest = string.IsNullOrWhiteSpace(customRequest)
            ? null
            : customRequest.Trim();

        var request = new Request
        {
            EmployerId = employerId,
            WorkerId = workerId,
            Reason = reason.Trim(),
            ExpiryDate = null,
            CustomRequest = trimmedCustomRequest,
            CustomRequestStatus = trimmedCustomRequest == null ? null : "pending"
        };

        _dbContext.Requests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }

    public Task<LinkedList<Request>> GetAllByEmployerIdAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public async Task<List<Request>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Requests
            .Where(x => x.WorkerId == workerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Request> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Requests
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken)
            ?? throw new KeyNotFoundException();
    }
}
