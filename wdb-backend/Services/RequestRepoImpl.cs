using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestRepoImpl : IRequestRepository
{
    private readonly AppDbContext _dbContext;

    public RequestRepoImpl (AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task AddAsync(Guid employerId, Guid workerId, Request request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.Requests.Where(x => x.WorkerId == workerId).ToListAsync(cancellationToken);
        return result;
    }


}
