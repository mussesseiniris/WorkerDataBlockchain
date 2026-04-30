using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;


namespace wdb_backend.Services;

public class EmployerRepoImpl : IEmployerRepository
{
    private readonly AppDbContext _context;

    public EmployerRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    private readonly IEmployerRepository _employerRepository;


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

    //GetByIdAsync()
    //get all information of employer by employer id
    //name, email, blockchainaddress and privatekey of a employer can be extracted from this method

    public async Task<Employer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employers.FirstOrDefaultAsync(employer => employer.Id == id, cancellationToken);
    }

}
