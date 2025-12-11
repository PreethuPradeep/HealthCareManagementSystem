using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationRepository _consultationRepo;

        public ConsultationsController(IConsultationRepository consultationRepo)
        {
            _consultationRepo = consultationRepo;
        }

        // GET: Patient History
        [HttpGet("patient/{patientId}/history")]
        [Authorize(Roles = "Admin,Doctor,Receptionist")] // Restrict patient history access to authorized roles
        public async Task<IActionResult> GetHistory(int patientId)
        {
            if (patientId <= 0)
                return BadRequest("Invalid patient ID");

            var history = await _consultationRepo.GetPatientHistoryAsync(patientId);
            return Ok(history);
        }

        // GET: Patient History with prescriptions and lab tests (for doctor view)
        [HttpGet("patient/{patientId}/history/details")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetDetailedHistory(int patientId)
        {
            if (patientId <= 0)
                return BadRequest("Invalid patient ID");

            var history = await _consultationRepo.GetPatientHistoryWithDetailsAsync(patientId);
            return Ok(history);
        }

        // POST: Create Consultation
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateConsultation([FromBody] ConsultationRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _consultationRepo.AddConsultationAsync(request);
                return Ok(new
                {
                    Message = "Consultation saved successfully",
                    ConsultationId = result.ConsultationId
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred while saving the consultation.");
            }
        }

        // PUT: Update Consultation
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] ConsultationRequestDTO request)
        {
            if (id <= 0)
                return BadRequest("Invalid consultation ID.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _consultationRepo.UpdateConsultationAsync(id, request);

            if (!success)
                return NotFound("Consultation not found");

            return Ok(new { Message = "Consultation updated successfully" });
        }

        // DELETE: Delete Consultation
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConsultation(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid consultation ID.");

            var success = await _consultationRepo.DeleteConsultationAsync(id);

            if (!success)
                return NotFound("Consultation not found");

            return Ok(new { Message = "Consultation deleted successfully" });
        }
    }
}
