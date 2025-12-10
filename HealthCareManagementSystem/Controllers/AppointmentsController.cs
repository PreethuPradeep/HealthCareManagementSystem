using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentsController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetAppointmentsByDoctor(int doctorId, [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;

        var appointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, targetDate);

        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId}/pending")]
    public async Task<IActionResult> GetPendingAppointments(int doctorId, [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;

        var appointments = await _appointmentRepository.GetPendingByDoctorAndDateAsync(doctorId, targetDate);

        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientAsync(patientId);
        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment(Appointment appointment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _appointmentRepository.AddAsync(appointment);

        return CreatedAtAction(nameof(GetAppointment),
            new { id = created.AppointmentId },
            created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _appointmentRepository.UpdateAsync(id, appointment);

        if (updated == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var deleted = await _appointmentRepository.DeleteAsync(id);

        if (!deleted)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(new { Message = "Appointment deleted" });
    }
}
