using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IRequestRepository
{

    // create request
    Task AddAsync(Guid employerId, Guid workerId, Request request, CancellationToken cancellationToken = default);

    // query all requests by employer id
    Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default);

    // query all requests by worker id
    Task<List<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<Request> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
