namespace wdb_backend.Abstractions;

public interface IWorkerDashboardService
{
    Task<object?> GetDashboardAsync(Guid workerId, CancellationToken cancellationToken = default);
}
