using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using System.Security.Claims;

namespace wdb_backend.Controllers
{
    /// <summary>
    /// API controller for managing worker information operations.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/worker/info")]
    public class WorkerInfoController : ControllerBase
    {
        private readonly IWorkerInfoService _workerInfoService;

        public WorkerInfoController(IWorkerInfoService workerInfoService)
        {
            _workerInfoService = workerInfoService;
        }

        /// <summary>
        /// helper method to extract the current worker's id from the jwt token.
        /// </summary>
        private Guid GetCurrentWorkerId()
        {
            // ✅ 关键:JwtRegisteredClaimNames.Sub 对应 ClaimTypes.NameIdentifier
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)  // ← 用这个!
                     ?? User.FindFirst("sub");  // ← 备用

            if (claim == null)
            {
                // log for debugging
                Console.WriteLine("❌ User ID claim not found. Available claims:");
                foreach (var c in User.Claims)
                {
                    Console.WriteLine($"  {c.Type}: {c.Value}");
                }
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            Console.WriteLine($"✅ Found claim: {claim.Type} = {claim.Value}");
            return Guid.Parse(claim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var workerId = GetCurrentWorkerId();
                var infos = await _workerInfoService.GetAllAsync(workerId);
                return Ok(infos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetAll failed: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkerInfo info)
        {
            try
            {
                var workerId = GetCurrentWorkerId();
                var created = await _workerInfoService.CreateAsync(workerId, info);
                return Ok(created);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Create failed: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] WorkerInfo info)
        {
            try
            {
                var workerId = GetCurrentWorkerId();

                // log for debugging
                Console.WriteLine($"=== Update WorkerInfo ===");
                Console.WriteLine($"WorkerId: {workerId}");
                Console.WriteLine($"Desc: {info.Desc}");
                Console.WriteLine($"Value: {info.Value}");

                var updated = await _workerInfoService.UpdateAsync(workerId, info);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Update failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}


