using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;



// this class is define the implementation of worker info repository.
public class WorkerInfoRepoImpl : IWorkerInfoRepository
{
    private readonly AppDbContext _context;

    public WorkerInfoRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    public Task<WorkerInfo> GetOneAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }




    // This method receive a workerid and return all the worker info related to paramenter workerid.
    // the parameter workerid is the key in the database, cacellation token is to cancale the request if user pasue the request.
    // the return type is a hashset.
    public async Task<HashSet<WorkerInfo>> GetAllAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        var workerInfos = await _context.WorkerInfos
           .Where(w => w.WorkerId == workerId)
           .ToListAsync<WorkerInfo>(cancellationToken);// use .tolistasync to transform data from database to list of workerinfo that match c# grammar.

        return workerInfos.ToHashSet(); // transform list to hashset.
    }



    // this method is to add a worker info to database, worker info is the parameter that use want to add.
    public Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        workerInfo.WorkerId = workerId;// get the user input info.
        _context.WorkerInfos.Add(workerInfo); // target the specific table in db and add passed info to the table.
        return _context.SaveChangesAsync(cancellationToken);// implement the change in db and keep the change until request finished/cancelled.
    }



    // this method is to make the user's update operation can be saved in db.
    // the return type is worker info.
    public async Task<WorkerInfo> UpdateAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        var exsitingWorkerInfo = _context.WorkerInfos.FirstOrDefault(w => w.Id == workerInfo.Id && w.WorkerId == workerId);// check if the worker info exist in db.
        if (exsitingWorkerInfo == null)
        {
            throw new KeyNotFoundException($"WorkerInfo with id {workerInfo.Id} for worker {workerId} not fount.");
        } // if not exit,throw a warning to user.

        exsitingWorkerInfo.Desc = workerInfo.Desc; // if exit, let the user updated info to be saved.
        exsitingWorkerInfo.Value = workerInfo.Value; //let user updated info to be saved.
        await _context.SaveChangesAsync(cancellationToken);// implement the update in db and keep the updated info until request finised/cancelled.
        return exsitingWorkerInfo;
    }



    // this method is to delete the whole worker info in db.
    public async Task DeleteAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        // find the worker info by worker id
        var allInfos = await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .ToListAsync(cancellationToken);

        if (!allInfos.Any()) throw new KeyNotFoundException("Worker info not found.");

        // delete the worker info
        _context.WorkerInfos.RemoveRange(allInfos);
        await _context.SaveChangesAsync(cancellationToken);
    }

}
