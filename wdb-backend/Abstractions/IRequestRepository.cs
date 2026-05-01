using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IRequestRepository
{

    // create request
    Task<Request> AddAsync(Guid employerId, Guid workerId, string reason, CancellationToken cancellationToken = default);

    // query all requests by employer id
    Task<List<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default);

    // query all requests by worker id
    Task<LinkedList<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);
}
