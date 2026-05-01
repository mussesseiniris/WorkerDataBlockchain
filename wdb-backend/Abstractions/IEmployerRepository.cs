using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IEmployerRepository : IUserRepository<Employer>
{
    // // check whether the email/account exists
    // Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    //
    // // get employer by email
    // Task<Employer> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    //
    // // add employer
    // Task AddAsync(Employer employer, CancellationToken cancellationToken = default);
    //
    // // update employer by email
    // Task<Employer> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default);
    //
    // // delete employer by email
    // Task<Employer> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Employer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
