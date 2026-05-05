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

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Workers.AnyAsync(worker => worker.Email.Equals(email), cancellationToken);
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

    public async Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        await _context.Workers.AddAsync(worker, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
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
