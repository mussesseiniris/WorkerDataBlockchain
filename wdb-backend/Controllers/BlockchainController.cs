using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for managing blockchain transaction log operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BlockchainController : ControllerBase
{
    private readonly IBlockchainService _blockchainService;

    public BlockchainController(IBlockchainService blockchainService)
    {
        _blockchainService = blockchainService;
    }

    /// <summary>
    /// Writes a transaction log entry to the blockchain.
    /// Called when a permission action occurs.
    /// </summary>
    /// <param name="request">The transaction details to log on the blockchain.</param>
    /// <returns>200 OK with the transaction hash as proof of the log.</returns>
    [HttpPost("log")]
    public async Task<ActionResult<string>> LogTransaction(LogTransactionRequest request)
    {
        var txHash = await _blockchainService.LogTransactionAsync(
            privateKey: request.PrivateKey,
            employerAddress: request.EmployerAddress,
            workerAddress: request.WorkerAddress,
            action: request.Action
        );

        return Ok(txHash);
    }

    /// <summary>
    /// Returns all blockchain logs for a specific worker.
    /// </summary>
    /// <param name="address">The blockchain address of the worker.</param>
    /// <returns>200 OK with a list of all transaction logs involving this worker.</returns>
    [HttpGet("logs/worker/{address}")]
    public async Task<ActionResult<List<BlockchainTransactionResponse>>> GetWorkerLogs(string address)
    {
        var logs = await _blockchainService.GetWorkerLogsAsync(address);
        return Ok(logs);
    }

    /// <summary>
    /// Returns all blockchain logs for a specific employer.
    /// </summary>
    /// <param name="address">The blockchain address of the employer.</param>
    /// <returns>200 OK with a list of all transaction logs involving this employer.</returns>
    [HttpGet("logs/employer/{address}")]
    public async Task<ActionResult<List<BlockchainTransactionResponse>>> GetEmployerLogs(string address)
    {
        var logs = await _blockchainService.GetEmployerLogsAsync(address);
        return Ok(logs);
    }
}

/// <summary>
/// Request body for logging a blockchain transaction.
/// </summary>
public class LogTransactionRequest
{
    public string PrivateKey { get; set; } = string.Empty;
    public string EmployerAddress { get; set; } = string.Empty;
    public string WorkerAddress { get; set; } = string.Empty;
    public BlockchainAction Action { get; set; }
}