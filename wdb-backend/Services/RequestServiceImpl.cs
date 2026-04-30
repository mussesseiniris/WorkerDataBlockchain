using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestServiceImpl:IRequestService
{
    private readonly IRequestRepository _requestRepository;

    public RequestServiceImpl(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

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

    public async Task <Request> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var result = await _requestRepository.GetByRequestIdAsync(requestId, cancellationToken);
        return result;
    }

}
