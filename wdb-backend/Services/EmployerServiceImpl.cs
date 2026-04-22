using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerServiceImpl:IEmployerService
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> CreateAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> UpdateAsync(string email, Employer employer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> DeleteAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
