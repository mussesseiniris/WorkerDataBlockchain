using Microsoft.AspNetCore.Mvc;
using wdb_backend.Data;
using wdb_backend.Models;
namespace wdb_backend.Controllers;

/// <summary>
/// API controller for managing worker-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkerController:ControllerBase
{
    private readonly AppDbContext _context;
    public WorkerController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new worker record in the database.
    /// </summary>
    /// <param name="worker">The worker data to be stored in the database.</param>
    /// <returns>200 OK with the created worker, including database-generated ID and timestamp.</returns>
    [HttpPost]
    public async Task<ActionResult<Worker>> AddWorker(Worker worker)
    {
        _context.Workers.Add(worker);
        await _context.SaveChangesAsync();
        return Ok(worker);
    }
}
