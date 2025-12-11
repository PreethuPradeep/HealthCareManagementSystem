using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LabTestsController : ControllerBase
    {
        private readonly HealthCareDbContext _context;

        public LabTestsController(HealthCareDbContext context)
        {
            _context = context;
        }

        // GET: api/LabTests/patient/5
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Admin,Doctor,Lab")] // Restrict lab test access to authorized roles
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var tests = await _context.LabTestRequests
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return Ok(tests);
        }

        // PUT: api/LabTests/5  (Update status + result)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateLabTest(int id, LabTestRequest request)
        {
            if (id != request.LabTestRequestId)
                return BadRequest("LabTestRequest ID mismatch.");

            var existing = await _context.LabTestRequests.FindAsync(id);
            if (existing == null)
                return NotFound("Lab test not found.");

            // Editable fields
            existing.TestName = request.TestName;
            existing.Status = request.Status;
            existing.Result = request.Result;

            if (request.Status == "Completed" && existing.CompletedAt == null)
                existing.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Lab Test Updated Successfully" });
        }

        // DELETE: api/LabTests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLabTest(int id)
        {
            var test = await _context.LabTestRequests.FindAsync(id);
            if (test == null)
                return NotFound("Lab test not found.");

            _context.LabTestRequests.Remove(test);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Lab Test Deleted Successfully" });
        }
    }
}
