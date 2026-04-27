using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;

// API endpoints for CRUD operations on worker information will be implemented here.
namespace wdb_backend.Controllers
{
    /// <summary>
    /// API controller for managing worker information operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerInfoController : ControllerBase
    {
        private readonly IWorkerInfoRepository _workerInfoRepository;
        

        public WorkerInfoController(IWorkerInfoRepository workerInfoRepository)
        {
            _workerInfoRepository = workerInfoRepository;
        }

        /// <summary>
        /// Gets all information for a specific worker.
        /// </summary>
        [HttpGet("{workerId}")]
        public async Task<ActionResult<HashSet<WorkerInfo>>> GetAllWorkerInfo(Guid workerId)
        {
            var workerInfo = await _workerInfoRepository.GetAllAsync(workerId);
            return Ok(workerInfo);
        }

        /// <summary>
        /// add new info to worker info table.
        /// </summary>
        [HttpPost("{workerId}")]
        public async Task<ActionResult> AddWorkerInfo(Guid workerId, WorkerInfo workerInfo)
        {
            await _workerInfoRepository.AddOneAsync(workerId, workerInfo);
            return Ok();
        }

        /// <summary>
        /// update edited info to worker info table.
        /// </summary>
        [HttpPut("{workerId}")]
        public async Task<ActionResult<WorkerInfo>> UpdateWorkerInfo(Guid workerId, WorkerInfo workerInfo)
        {
            var updatedInfo = await _workerInfoRepository.UpdateAsync(workerId, workerInfo);
            return Ok(updatedInfo);
        }

        /// <summary>
        /// Deletes all worker info for a specific worker.
        /// </summary>
        [HttpDelete("{workerId}")]
        public async Task<ActionResult> DeleteWorkerInfo(Guid workerId)
        {
            await _workerInfoRepository.DeleteAsync(workerId);
            return Ok();
        }
    }

}


