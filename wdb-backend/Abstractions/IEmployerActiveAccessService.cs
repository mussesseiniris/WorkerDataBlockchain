using wdb_backend.DTOs;

namespace wdb_backend.Abstractions;

public interface IEmployerActiveAccessService
{
    Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default);

    // E4 request-level view:
    // validates ownership/status/expiry and returns all approved text values
    // and short-lived Supabase signed URLs for file items under the request.
    Task<EmployerRequestAccessViewDto> ViewRequestAsync(
        Guid employerId,
        Guid requestId,
        CancellationToken cancellationToken = default);
}
