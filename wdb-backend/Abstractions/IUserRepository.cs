namespace wdb_backend.Abstractions;

public interface IUserRepository<T> where T : class, IUser
{
    // check whether the email/account exists
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    // get user by email
    Task<T?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // add user
    Task AddAsync(T user, CancellationToken cancellationToken = default);

    // update user by email
    Task<T> UpdateByEmailAsync(string email, CancellationToken cancellationToken = default);

    // delete user by email
    Task<T> DeleteByEmailAsync(string email, CancellationToken cancellationToken = default);

}
