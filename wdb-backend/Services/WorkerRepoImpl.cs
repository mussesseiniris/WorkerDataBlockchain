using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerRepoImpl : IWorkerRepository
{
    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<HashSet<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> DeleteAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> GetWorkerInfoById(Guid workerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
