using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IWorkerInfoRepository
{
    // get specific worker info
    Task<WorkerInfo> GetOneAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default);

    // get all worker info of specific worker
    Task<HashSet<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default);

    // add worker info by worker id
    Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    // update worker info by worker info id
    Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    // delete worker info by worker info id
    Task<WorkerInfo> DeleteAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default);
}
