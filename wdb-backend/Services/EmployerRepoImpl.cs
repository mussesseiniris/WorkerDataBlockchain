using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerRepoImpl:IEmployerRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Employer> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
