using wdb_backend.Models;

namespace wdb_backend.Abstractions;

public interface IBlockchainService
{
    BlockchainKeyPair GenerateKeyPair();

    Task<string> LogTransactionAsync(string privateKey, string employerAddress, string workerAddress, BlockchainAction action, CancellationToken cancellationToken = default);

    Task<List<BlockchainTransactionResponse>> GetWorkerLogsAsync(string workerAddress, CancellationToken cancellationToken = default);

    Task<List<BlockchainTransactionResponse>> GetEmployerLogsAsync(string employerAddress, CancellationToken cancellationToken = default);
}