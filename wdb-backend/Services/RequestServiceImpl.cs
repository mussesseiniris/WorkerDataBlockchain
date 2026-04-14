using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestServiceImpl:IRequestService
{
    public Task<Request> CreateAsync(Guid employerId, Guid workerId, Request request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LinkedList<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
