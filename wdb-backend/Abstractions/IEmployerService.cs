using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IEmployerService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<Employer> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Employer> CreateAsync(Employer employer, CancellationToken cancellationToken = default);

    Task<Employer> UpdateAsync(string email, Employer employer, CancellationToken cancellationToken = default);

    Task<Employer> DeleteAsync(string email, CancellationToken cancellationToken = default);
}
