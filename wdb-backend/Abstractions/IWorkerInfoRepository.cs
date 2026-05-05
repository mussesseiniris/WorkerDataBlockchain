using wdb_backend.Models;

namespace wdb_backend.Abstractions;


// this interface for worker info repository,which is manageing the worker info data in db and provide what the service layer need to follow up.
public interface IWorkerInfoRepository
{
    // get specific worker info
    Task<WorkerInfo> GetOneAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default);

    // get all worker info of specific worker
    Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default);

    // add worker info by worker id
    Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    // update worker info by worker info id
    Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default);

    // delete the whole worker info by worker info id，have not start since ui have not define.
    //Task DeleteAsync(Guid workerId, CancellationToken cancellationToken = default);
}
