using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static HealthCareManagementSystem.Repository.IPatientRepository;

namespace HealthCareManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Receptionist,Doctor")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Patient>>> GetPatients(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "MRN")
    {
        var sortByEnum = Enum.TryParse<PatientSortBy>(sortBy, true, out var parsed) 
            ? parsed 
            : PatientSortBy.MRN;
        
        var result = await _patientRepository.GetAllAsync(pageNumber, pageSize, sortByEnum);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);

        if (patient == null)
            return NotFound($"Patient with ID {id} not found.");

        return Ok(patient);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "MRN")
    {
        var sortByEnum = Enum.TryParse<PatientSortBy>(sortBy, true, out var parsed) 
            ? parsed 
            : PatientSortBy.MRN;
        
        var result = await _patientRepository.SearchAsync(searchTerm, pageNumber, pageSize, sortByEnum);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Patient>> CreatePatient(Patient patient)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdPatient = await _patientRepository.AddAsync(patient);
        return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.PatientId }, createdPatient);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(int id, Patient patient)
    {
        if (id != patient.PatientId)
            return BadRequest("Patient ID mismatch.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _patientRepository.UpdateAsync(id, patient);
        if (updated == null)
            return NotFound($"Patient with ID {id} not found.");

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var deleted = await _patientRepository.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Patient with ID {id} not found.");

        return Ok(new { Message = "Patient deleted" });
    }
}
