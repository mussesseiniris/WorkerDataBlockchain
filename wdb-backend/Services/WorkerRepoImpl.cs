using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class WorkerRepoImpl : IWorkerRepository
{
    private readonly AppDbContext _context;

    public WorkerRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    public Task AddOneAsync(Guid workerId, WorkerInfo workerInfo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Worker> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var resultWorker = await _context.Workers.FirstOrDefaultAsync(w => w.Email == email, cancellationToken);
        if (resultWorker == null)
        {
            throw new KeyNotFoundException($"Worker with {email} email not found");
        }

        return resultWorker;
    }

    public Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Worker> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
