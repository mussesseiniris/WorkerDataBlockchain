using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerDashboardService
{
    Task<EmployerDashboardDto> GetDashboardAsync(
        Guid employerId,
        CancellationToken cancellationToken = default
    );
}
