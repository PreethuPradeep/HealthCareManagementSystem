using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabTestsController : ControllerBase
    {
        private readonly HealthCareDbContext _context;

        public LabTestsController(HealthCareDbContext context)
        {
            _context = context;
        }

        // GET: api/LabTests/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var tests = await _context.LabTestRequests
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();
            return Ok(tests);
        }

        // PUT: api/LabTests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLabTest(int id, LabTestRequest test)
        {
            if (id != test.LabTestRequestId) return BadRequest();

            _context.Entry(test).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Lab Test Updated" });
        }

        // DELETE: api/LabTests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabTest(int id)
        {
            var test = await _context.LabTestRequests.FindAsync(id);
            if (test == null) return NotFound();

            _context.LabTestRequests.Remove(test);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Lab Test Deleted" });
        }
    }
}
