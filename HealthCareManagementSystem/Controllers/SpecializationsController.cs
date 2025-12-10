using Microsoft.AspNetCore.Mvc;
using HealthCareManagementSystem.Repository;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationRepository _specializationRepository;

        public SpecializationsController(ISpecializationRepository specializationRepository)
        {
            _specializationRepository = specializationRepository;
        }

        // GET: api/specializations
        [HttpGet]
        public async Task<IActionResult> GetActiveSpecializations()
        {
            var specializations = await _specializationRepository.GetAllActiveSpecializationsAsync();

            return Ok(specializations.Select(s => new
            {
                s.SpecializationId,
                s.SpecializationName,
                s.IsActive
            }));
        }

        // GET: api/specializations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecializationById(int id)
        {
            var specialization = await _specializationRepository.GetSpecializationByIdAsync(id);

            if (specialization == null)
                return NotFound(new { Message = $"Specialization with ID {id} not found" });

            return Ok(new
            {
                specialization.SpecializationId,
                specialization.SpecializationName,
                specialization.IsActive
            });
        }

        // POST: api/specializations
        [HttpPost]
        public async Task<IActionResult> CreateSpecialization(Specialization specialization)
        {
            specialization.IsActive = true;

            await _specializationRepository.AddSpecializationAsync(specialization);
            return Ok(new { Message = "Specialization added successfully" });
        }

        // PUT: api/specializations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpecialization(int id, Specialization specialization)
        {
            if (id != specialization.SpecializationId)
                return BadRequest(new { Message = "Specialization ID mismatch" });

            var result = await _specializationRepository.UpdateSpecializationAsync(specialization);

            if (result > 0)
                return Ok(new { Message = "Specialization updated successfully" });

            return NotFound(new { Message = $"Specialization with ID {id} not found" });
        }

        // DELETE: api/specializations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialization(int id)
        {
            var result = await _specializationRepository.DeactivateSpecializationAsync(id);

            if (result > 0)
                return Ok(new { Message = $"Specialization with ID {id} is deactivated" });

            return NotFound(new { Message = $"Specialization with ID {id} not found" });
        }
    }
}
