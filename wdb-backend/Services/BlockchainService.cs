using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using System.Text.Json;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

// ─────────────────────────────────────────────────────
// Maps to the Solidity event 
// event TransactionLog(
//   address indexed employer,
//   address indexed worker,
//   uint256 date,
//   uint8   action
// )
// ─────────────────────────────────────────────────────
[Event("TransactionLog")]
public class TransactionLogEvent : IEventDTO
{
    [Parameter("address", "employer", 1, true)]
    public string EmployerAddress { get; set; } = string.Empty;

    [Parameter("address", "worker", 2, true)]
    public string WorkerAddress { get; set; } = string.Empty;

    [Parameter("uint256", "date", 3, false)]
    public long Date { get; set; }

    [Parameter("uint8", "action", 4, false)]
    public int Action { get; set; }
}

public class BlockchainService : IBlockchainService
{
    private readonly string _rpcUrl;
    private readonly string _contractAddress;
    private readonly string _abiPath;
    private string? _abi;
    private readonly ILogger<BlockchainService> _logger;

    public BlockchainService(
        IConfiguration config,
        ILogger<BlockchainService> logger)
    {
        _logger = logger;
        _rpcUrl = config["Blockchain:RpcUrl"]
            ?? throw new InvalidOperationException("Blockchain:RpcUrl not configured");
        _contractAddress = config["Blockchain:ContractAddress"]
            ?? throw new InvalidOperationException("Blockchain:ContractAddress not configured");

        var abiPath = config["Blockchain:AbiPath"]
            ?? throw new InvalidOperationException("Blockchain:AbiPath not configured");

        _abiPath = abiPath;
    }

    private string GetAbi()
    {
        if (_abi != null) return _abi;

        var artifact = JsonSerializer.Deserialize<JsonElement>(
            File.ReadAllText(_abiPath)
        );
        _abi = artifact.GetProperty("abi").GetRawText();
        return _abi;
    }

    // ─────────────────────────────────────────────────────
    // Generates a new blockchain key pair
    // Pure math — no network call, no async needed
    // Called when a new worker or employer registers
    // ─────────────────────────────────────────────────────
    public BlockchainKeyPair GenerateKeyPair()
    {
        var key = EthECKey.GenerateKey();

        return new BlockchainKeyPair
        {
            PrivateKey = key.GetPrivateKey(),
            BlockchainAddress = key.GetPublicAddress()
        };
    }

    // ─────────────────────────────────────────────────────
    // WRITE — sends a signed transaction to blockchain
    // Private key comes from the database (employer's key)
    // Returns TxHash as proof the log was recorded
    // ─────────────────────────────────────────────────────
    public async Task<string> LogTransactionAsync(
        string privateKey,
        string employerAddress,
        string workerAddress,
        BlockchainAction action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = new Account(privateKey);
            var web3 = new Web3(account, _rpcUrl);
            var contract = web3.Eth.GetContract(_abi, _contractAddress);
            var logFn = contract.GetFunction("logTransaction");

            var txHash = await logFn.SendTransactionAsync(
                from: account.Address,
                gas: null,
                value: null,
                functionInput: new object[]
                {
                    employerAddress,
                    workerAddress,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    (int)action
                }
            );

            _logger.LogInformation(
                "Blockchain log written — Action: {Action} | TxHash: {TxHash}",
                action, txHash
            );

            return txHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to write blockchain log — Action: {Action}", action);
            throw;
        }
    }

    // ─────────────────────────────────────────────────────
    // READ — fetches all logs for a specific worker
    // No private key needed — read only
    // ─────────────────────────────────────────────────────
    public async Task<List<BlockchainTransactionResponse>> GetWorkerLogsAsync(
        string workerAddress,
        CancellationToken cancellationToken = default)
    {
        return await QueryLogsAsync(workerAddress: workerAddress);
    }

    // ─────────────────────────────────────────────────────
    // READ — fetches all logs for a specific employer
    // No private key needed — read only
    // ─────────────────────────────────────────────────────
    public async Task<List<BlockchainTransactionResponse>> GetEmployerLogsAsync(
        string employerAddress,
        CancellationToken cancellationToken = default)
    {
        return await QueryLogsAsync(employerAddress: employerAddress);
    }

    // ─────────────────────────────────────────────────────
    // Shared internal query logic
    // Filters events by worker or employer address
    // ─────────────────────────────────────────────────────
    private async Task<List<BlockchainTransactionResponse>> QueryLogsAsync(
        string? workerAddress = null,
        string? employerAddress = null)
    {
        try
        {
            var web3 = new Web3(_rpcUrl);
            var contract = web3.Eth.GetContract(GetAbi(), _contractAddress);

            var eventHandler = contract.GetEvent<TransactionLogEvent>();
            var filterInput = eventHandler.CreateFilterInput(
                fromBlock: BlockParameter.CreateEarliest(),
                toBlock: BlockParameter.CreateLatest()
            );

            var events = await eventHandler.GetAllChangesAsync(filterInput);

            return events
                .Where(e =>
                    (workerAddress == null || e.Event.WorkerAddress.Equals(
                        workerAddress, StringComparison.OrdinalIgnoreCase)) &&
                    (employerAddress == null || e.Event.EmployerAddress.Equals(
                        employerAddress, StringComparison.OrdinalIgnoreCase))
                )
                .Select(e => new BlockchainTransactionResponse
                {
                    EmployerAddress = e.Event.EmployerAddress,
                    WorkerAddress = e.Event.WorkerAddress,
                    Date = DateTimeOffset
                                        .FromUnixTimeSeconds(e.Event.Date)
                                        .UtcDateTime,
                    Action = ((BlockchainAction)e.Event.Action).ToString(),
                    TxHash = e.Log.TransactionHash
                })
                .OrderByDescending(e => e.Date)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query blockchain logs");
            throw;
        }
    }
}