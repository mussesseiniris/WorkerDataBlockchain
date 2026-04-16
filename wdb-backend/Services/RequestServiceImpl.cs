using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class RequestServiceImpl : IRequestService
{
    private readonly IRequestRepository _requestRepo;
    public RequestServiceImpl(IRequestRepository requestRepo){
        _requestRepo = requestRepo;
    }
    public async Task<Request> CreateAsync(Guid employerId, Guid workerId, string reason, CancellationToken cancellationToken = default)
    {
        var resultRequest = await _requestRepo.AddAsync(employerId, workerId, reason, default) ?? throw new KeyNotFoundException();
        return resultRequest;
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
