using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerRepoImpl:IEmployerRepository
{
    private readonly AppDbContext _context;

    public EmployerRepoImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Employers.AnyAsync(employer => employer.Email == email, cancellationToken);
    }

    public async Task<Employer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Employers.FirstOrDefaultAsync(employer => employer.Email.Equals(email), cancellationToken);
    }

    public async Task AddAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        await _context.Employers.AddAsync(employer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
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
