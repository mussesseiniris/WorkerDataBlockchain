using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IWorkerService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<Worker> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Worker> CreateAsync(Worker worker, CancellationToken cancellationToken = default);

    Task<Worker> UpdateByEmailAsync(string email, Worker worker, CancellationToken cancellationToken = default);

    Task<Worker> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<WorkerInfo> GetWorkerInfoByIdAsync(Guid workerId, CancellationToken cancellationToken = default);
}
