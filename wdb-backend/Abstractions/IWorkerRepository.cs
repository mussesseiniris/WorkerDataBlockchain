using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IWorkerRepository : IUserRepository<Worker>
{
    // // check whether the email/account exists
    // Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    //
    // // get worker by email
    // Task<Worker> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    //
    // // add worker
    // Task AddAsync(Worker worker, CancellationToken cancellationToken = default);
    //
    // // update worker by email
    // Task<Worker> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default);
    //
    // // delete worker by email
    // Task<Worker> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default);


}
