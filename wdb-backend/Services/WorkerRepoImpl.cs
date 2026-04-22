using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerRepoImpl:IWorkerInfoRepository
{
    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
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

    public Task<WorkerInfo> DeleteAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> GetByEmailAsync(string workerEmail, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
