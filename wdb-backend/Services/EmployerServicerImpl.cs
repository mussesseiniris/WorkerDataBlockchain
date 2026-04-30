using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerServicerImpl : IEmployerService
{
    private readonly IEmployerRepository _employerRepository;

    public EmployerServicerImpl(IEmployerRepository employerRepository)
    {
        _employerRepository = employerRepository;
    }

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

    public async Task<Employer?> GetEmployerInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _employerRepository.GetByIdAsync(id, cancellationToken);
    }
}
