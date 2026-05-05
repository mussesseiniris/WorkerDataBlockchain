using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerServiceImpl : IWorkerService
{

    private readonly IWorkerRepository _workerRepository;

    public WorkerServiceImpl(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Worker> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {

        var resultWorker =await _workerRepository.GetByEmailAsync(email, default)??throw new KeyNotFoundException();
        return resultWorker;
    }

    public Task<Worker> CreateAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> UpdateByEmailAsync(string email, Worker worker, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}
