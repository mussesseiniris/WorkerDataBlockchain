using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IRequestService
{
    Task<Request> CreateAsync(Guid employerId, Guid workerId, string reason, CancellationToken cancellationToken = default);

    Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default);

    Task<List<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<Request> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);

}
