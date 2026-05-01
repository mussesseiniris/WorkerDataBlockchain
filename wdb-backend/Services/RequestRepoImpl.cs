using Microsoft.AspNetCore.Http.HttpResults;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestRepoImpl : IRequestRepository
{

    private readonly AppDbContext _context;
    public RequestRepoImpl(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Request> AddAsync(Guid employerId, Guid workerId, string reason, CancellationToken cancellationToken = default)
    {
        var request = new Request { EmployerId = employerId, WorkerId = workerId, Reason = reason };
        _context.Requests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }


    public Task<List<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
