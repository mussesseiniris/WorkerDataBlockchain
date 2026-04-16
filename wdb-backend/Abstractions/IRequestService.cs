using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IRequestService
{
    Task<Request> CreateAsync(Guid employerId, Guid workerId, Request request, CancellationToken cancellationToken = default);

    Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default);

    Task<LinkedList<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<List<Request>> GetAllByRequestIdAsync(Guid workerid, Guid requestId, CancellationToken cancellationToken = default);

}
