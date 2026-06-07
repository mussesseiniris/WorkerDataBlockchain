using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Text.Json;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Services;

/// <summary>
/// Maps to Solidity:
///
/// event TransactionLogged(
///   address indexed employer,
///   address indexed worker,
///   string requestId,
///   string category,
///   string permissionIds,
///   string itemLabels,
///   uint256 date,
///   Action action
/// )
/// </summary>
[Event("TransactionLogged")]
public class TransactionLogEvent : IEventDTO
{
    [Parameter("address", "employer", 1, true)]
    public string EmployerAddress { get; set; } = string.Empty;

    [Parameter("address", "worker", 2, true)]
    public string WorkerAddress { get; set; } = string.Empty;

    [Parameter("string", "requestId", 3, false)]
    public string RequestId { get; set; } = string.Empty;

    [Parameter("string", "category", 4, false)]
    public string Category { get; set; } = string.Empty;

    [Parameter("string", "permissionIds", 5, false)]
    public string PermissionIds { get; set; } = string.Empty;

    [Parameter("string", "itemLabels", 6, false)]
    public string ItemLabels { get; set; } = string.Empty;

    [Parameter("uint256", "date", 7, false)]
    public long Date { get; set; }

    [Parameter("uint8", "action", 8, false)]
    public int Action { get; set; }
}

public class BlockchainService : IBlockchainService
{
    private readonly string _rpcUrl;
    private readonly string _contractAddress;
    private readonly string _abiPath;
    private readonly ulong? _startBlock;
    private readonly ILogger<BlockchainService> _logger;

    private string? _abi;

    public BlockchainService(
        IConfiguration config,
        ILogger<BlockchainService> logger)
    {
        _logger = logger;

        _rpcUrl = config["Blockchain:RpcUrl"]
            ?? throw new InvalidOperationException("Blockchain:RpcUrl not configured");

        _contractAddress = config["Blockchain:ContractAddress"]
            ?? throw new InvalidOperationException("Blockchain:ContractAddress not configured");

        _abiPath = config["Blockchain:AbiPath"]
            ?? throw new InvalidOperationException("Blockchain:AbiPath not configured");

        _startBlock = ulong.TryParse(config["Blockchain:StartBlock"], out var sb) ? sb : null;
    }

    private string GetAbi()
    {
        if (_abi != null)
            return _abi;

        var artifact = JsonSerializer.Deserialize<JsonElement>(
            File.ReadAllText(_abiPath));

        _abi = artifact.GetProperty("abi").GetRawText();
        return _abi;
    }

    public BlockchainKeyPair GenerateKeyPair()
    {
        var key = EthECKey.GenerateKey();

        return new BlockchainKeyPair
        {
            PrivateKey = key.GetPrivateKey(),
            BlockchainAddress = key.GetPublicAddress()
        };
    }

    public Task<string> LogTransactionAsync(
        string privateKey,
        string employerAddress,
        string workerAddress,
        BlockchainAction action,
        CancellationToken cancellationToken = default)
    {
        return LogCategoryTransactionAsync(
            privateKey,
            employerAddress,
            workerAddress,
            requestId: string.Empty,
            category: "Unknown",
            permissionIds: string.Empty,
            itemLabels: string.Empty,
            action,
            cancellationToken);
    }

    public async Task<string> LogCategoryTransactionAsync(
        string privateKey,
        string employerAddress,
        string workerAddress,
        string requestId,
        string category,
        string permissionIds,
        string itemLabels,
        BlockchainAction action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new InvalidOperationException("Private key is required for blockchain logging.");

            if (string.IsNullOrWhiteSpace(employerAddress))
                throw new InvalidOperationException("Employer blockchain address is required.");

            if (string.IsNullOrWhiteSpace(workerAddress))
                throw new InvalidOperationException("Worker blockchain address is required.");

            var account = new Account(privateKey);
            var web3 = new Web3(account, _rpcUrl);
            var contract = web3.Eth.GetContract(GetAbi(), _contractAddress);
            var logFn = contract.GetFunction("logTransaction");

            // Contract calls need more than the default 21,000 gas.
            // 500,000 is safe for the local Hardhat demo environment.
            var gas = new HexBigInteger(500_000);

            var txHash = await logFn.SendTransactionAsync(
                from: account.Address,
                gas: gas,
                value: null,
                functionInput:
                [
                    employerAddress,
                    workerAddress,
                    requestId ?? string.Empty,
                    category ?? string.Empty,
                    permissionIds ?? string.Empty,
                    itemLabels ?? string.Empty,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    (int)action
                ]);

            _logger.LogInformation(
                "Blockchain category log written. Action={Action}, Category={Category}, RequestId={RequestId}, TxHash={TxHash}",
                action,
                category,
                requestId,
                txHash);

            return txHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write blockchain category log. Action={Action}, Category={Category}, RequestId={RequestId}",
                action,
                category,
                requestId);

            throw;
        }
    }

    public Task<List<BlockchainTransactionResponse>> GetWorkerLogsAsync(
        string workerAddress,
        CancellationToken cancellationToken = default)
    {
        return QueryLogsAsync(workerAddress: workerAddress);
    }

    public Task<List<BlockchainTransactionResponse>> GetEmployerLogsAsync(
        string employerAddress,
        CancellationToken cancellationToken = default)
    {
        return QueryLogsAsync(employerAddress: employerAddress);
    }

    private async Task<List<BlockchainTransactionResponse>> QueryLogsAsync(
        string? workerAddress = null,
        string? employerAddress = null)
    {
        try
        {
            var web3 = new Web3(_rpcUrl);
            var contract = web3.Eth.GetContract(GetAbi(), _contractAddress);

            var eventHandler = contract.GetEvent<TransactionLogEvent>();
            var fromBlock = _startBlock.HasValue
                ? new BlockParameter(new HexBigInteger(_startBlock.Value))
                : BlockParameter.CreateEarliest();
            var filterInput = eventHandler.CreateFilterInput(
                fromBlock: fromBlock,
                toBlock: BlockParameter.CreateLatest());

            var events = await eventHandler.GetAllChangesAsync(filterInput);

            return events
                .Where(e =>
                    (workerAddress == null ||
                     e.Event.WorkerAddress.Equals(workerAddress, StringComparison.OrdinalIgnoreCase)) &&
                    (employerAddress == null ||
                     e.Event.EmployerAddress.Equals(employerAddress, StringComparison.OrdinalIgnoreCase)))
                .Select(e => new BlockchainTransactionResponse
                {
                    EmployerAddress = e.Event.EmployerAddress,
                    WorkerAddress = e.Event.WorkerAddress,
                    RequestId = e.Event.RequestId,
                    Category = e.Event.Category,
                    PermissionIds = e.Event.PermissionIds,
                    ItemLabels = e.Event.ItemLabels,
                    Date = DateTimeOffset
                        .FromUnixTimeSeconds(e.Event.Date)
                        .UtcDateTime,
                    Action = ((BlockchainAction)e.Event.Action).ToString(),
                    TxHash = e.Log.TransactionHash,
                    BlockHash = e.Log.BlockHash
                })
                .OrderByDescending(e => e.Date)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query blockchain logs.");
            throw;
        }
    }
}
