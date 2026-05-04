using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestServiceImpl : IRequestService
{
    private readonly IRequestRepository _requestRepository;

    public RequestServiceImpl(IRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

   public async Task<Request> CreateAsync(Guid employerId, Guid workerId, string reason, CancellationToken cancellationToken = default)
    {
        var resultRequest = await _requestRepository.AddAsync(employerId, workerId, reason, default) ?? throw new KeyNotFoundException();
        return resultRequest;
    }


    public Task<LinkedList<Request>> GetAllByEmployerIdAsync(Guid employerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Request>> GetAllByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var result = await _requestRepository.GetAllByWorkerIdAsync(workerId, cancellationToken);
        return result;
    }

    public async Task <Request> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var result = await _requestRepository.GetByRequestIdAsync(requestId, cancellationToken);
        return result;
    }

}
