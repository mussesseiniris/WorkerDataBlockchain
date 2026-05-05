using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

/// <summary>
/// This class implements the IWorkerInfoService interface, providing business logic for managing worker information.
/// </summary>
public class WorkerInfoServiceImpl : IWorkerInfoService
{

    private readonly IWorkerInfoRepository _workerInfoRepo;



    public WorkerInfoServiceImpl(IWorkerInfoRepository workerInfoRepo)
    {
        _workerInfoRepo = workerInfoRepo;
    }


    /// <summary>
    /// This method retrieves a specific worker information record from the database based on the provided workerId and workerInfoId.
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="workerInfoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
    {
        return _workerInfoRepo.GetOneAsync(workerId, workerInfoId, cancellationToken);
    }

    public async Task<List<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var resultInfos = await _workerInfoRepo.GetAllAsync(workerId, default) ?? throw new KeyNotFoundException();
        return resultInfos;
    }

    /// <summary>
    /// This method retrieves all worker information records associated with a specific worker from the database.
    /// It takes the worker's unique identifier (workerId) as a parameter and returns a hashset of worker information records.
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HashSet<WorkerInfo>> GetAllAsyncHash(Guid workerId, CancellationToken cancellationToken = default)
    {
        return _workerInfoRepo.GetAllAsyncHash(workerId, cancellationToken);
    }

    /// <summary>
    /// This method is to create a new worker info record in the database for a specific worker.
    /// It takes the worker's unique identifier (workerId) and the worker info object to be created as parameters.
    /// The method calls the AddOneAsync method of the worker info repository to add the new record to the database, and then returns the created worker info object.
    /// </summary> <param name="workerId">The unique identifier of the worker for whom the info is being created.</param>
    /// <param name="workerInfo">The worker info object containing the details to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created worker info object.</returns>
    public async Task<WorkerInfo> CreateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        await _workerInfoRepo.AddOneAsync(workerId, workerInfo, cancellationToken);
        return workerInfo;
    }

    /// <summary>
    /// This method is to update an existing worker info record in the database for a specific worker. 
    /// It takes the worker's unique identifier (workerId) and the updated worker info object as parameters. 
    /// The method checks if a record with the same workerId and description already exists in the database. 
    /// If it exists, it updates the existing record with the new information; if it does not exist, it adds a new record to the database. Finally, it returns the updated or newly created worker info object.
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="workerInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        return _workerInfoRepo.UpdateAsync(workerId, workerInfo, cancellationToken);
    }

    /// <summary>
    /// 
    /// This method is to delete a worker info record from the database for a specific worker,but have not implemented yet.
    /// </summary>
    /// <param name="workerId"></param>
    /// <param name="workerInfoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>

    public Task<WorkerInfo> DeleteAsync(Guid workerId, Guid workerInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}