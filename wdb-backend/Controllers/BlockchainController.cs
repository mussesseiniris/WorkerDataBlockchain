using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Controllers;

/// <summary>
/// API controller for getting transaction logs.
/// </summary>
[ApiController]
[Route("api/blockchain")]
public class BlockchainController : ControllerBase
{
    private readonly IBlockchainService _blockchainService;

    public BlockchainController(IBlockchainService blockchainService)
    {
        _blockchainService = blockchainService;
    }

    [HttpGet("logs/worker/{address}")]
    public async Task<ActionResult<List<BlockchainTransactionResponse>>> GetWorkerLogs(string address)
    {
        var logs = await _blockchainService.GetWorkerLogsAsync(address);
        return Ok(logs);
    }

    [HttpGet("logs/employer/{address}")]
    public async Task<ActionResult<List<BlockchainTransactionResponse>>> GetEmployerLogs(string address)
    {
        var logs = await _blockchainService.GetEmployerLogsAsync(address);
        return Ok(logs);
    }
}