using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public class BillingsController : ControllerBase
    {
        private readonly IBillingRepository _billingRepository;

        public BillingsController(IBillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetBillings()
        {
            var billings = await _billingRepository.GetAllAsync();
            return Ok(billings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBilling(int id)
        {
            var billing = await _billingRepository.GetByIdAsync(id);
            if (billing == null)
                return NotFound($"Billing with ID {id} not found.");

            return Ok(billing);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetBillingsByPatient(int patientId)
        {
            var billings = await _billingRepository.GetByPatientAsync(patientId);
            return Ok(billings);
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetBillingByAppointment(int appointmentId)
        {
            var billing = await _billingRepository.GetByAppointmentIdAsync(appointmentId);
            if (billing == null)
                return NotFound($"Billing for appointment ID {appointmentId} not found.");

            return Ok(billing);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBilling(Billing billing)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _billingRepository.AddAsync(billing);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBilling(int id, Billing billing)
        {
            if (id != billing.BillingId)
                return BadRequest("Billing ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _billingRepository.UpdateAsync(id, billing);
            if (updated == null)
                return NotFound($"Billing with ID {id} not found.");

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilling(int id)
        {
            var deleted = await _billingRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound($"Billing with ID {id} not found.");

            return Ok(new { Message = "Billing deleted successfully" });
        }
    }
}
