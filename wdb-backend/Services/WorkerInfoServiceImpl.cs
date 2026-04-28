using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerInfoServiceImpl:IWorkerInfoService
{
    private readonly IWorkerInfoRepository _workerInfoRepo;
    public WorkerInfoServiceImpl(IWorkerInfoRepository workerInfoRepo)
    {
        _workerInfoRepo = workerInfoRepo;
    }
    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var resultInfos = await _workerInfoRepo.GetAllAsync(workerId,default)??throw new KeyNotFoundException();
        return resultInfos;
    }

    public Task<WorkerInfo> CreateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
