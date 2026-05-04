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
        // check if the record exist in db by workerid and desc, if exist then update the value,if not exist then add a new record in db.
        var existing = await _context.WorkerInfos
            .FirstOrDefaultAsync(w => w.WorkerId == workerId && w.Desc == workerInfo.Desc, cancellationToken);

        if (existing != null)
        {
            // if the record exist, then updat the vaule and save the change in db.
            existing.Value = workerInfo.Value;
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        else
        {
            // if it is not exist, then add a new record
            workerInfo.WorkerId = workerId;
            workerInfo.Id = Guid.NewGuid();  // generate a new id for the new record
            _context.WorkerInfos.Add(workerInfo);
            await _context.SaveChangesAsync(cancellationToken);
            return workerInfo;
        }
    }



    // this method is to delete the whole worker info in db. but ui have not define so this method have not done.
    public Task<WorkerInfo> DeleteAsync(Guid workerId, Guid wordInfoId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}
