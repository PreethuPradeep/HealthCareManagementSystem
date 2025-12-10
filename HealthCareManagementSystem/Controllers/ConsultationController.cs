using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationRepository _consultationRepo;

        public ConsultationsController(IConsultationRepository consultationRepo)
        {
            _consultationRepo = consultationRepo;
        }

        //get Patient History
        [HttpGet("patient/{patientId}/history")]
        public async Task<IActionResult> GetHistory(int patientId)
        {
            var history = await _consultationRepo.GetPatientHistoryAsync(patientId);
            return Ok(history);
        }

        // Start/Save Consultation
        [HttpPost]
        public async Task<IActionResult> CreateConsultation([FromBody] ConsultationRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _consultationRepo.AddConsultationAsync(request);
                return Ok(new { Message = "Consultation saved successfully", ConsultationId = result.ConsultationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // update Consultation
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] ConsultationRequestDTO request)
        {
            var success = await _consultationRepo.UpdateConsultationAsync(id, request);
            if (!success) return NotFound("Consultation not found");

            return Ok(new { Message = "Consultation updated successfully" });
        }

        // Delete Consultation
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultation(int id)
        {
            var success = await _consultationRepo.DeleteConsultationAsync(id);
            if (!success) return NotFound("Consultation not found");

            return Ok(new { Message = "Consultation deleted successfully" });
        }
    }
}
