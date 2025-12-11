using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // enforce authentication; actions declare role scopes explicitly
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentsController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // Admin + Receptionist: full appointment listing
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return Ok(appointments);
    }

    // Doctor (or Admin) can view their agenda for a given day
    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAppointmentsByDoctor(int doctorId, [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;

        var appointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, targetDate);

        return Ok(appointments);
    }

    // Doctor (or Admin) pending visits view
    [HttpGet("doctor/{doctorId}/pending")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetPendingAppointments(int doctorId, [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.Today;

        var appointments = await _appointmentRepository.GetPendingByDoctorAndDateAsync(doctorId, targetDate);

        return Ok(appointments);
    }

    // Common detail view
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetAppointment(int id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);

        if (appointment == null)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(appointment);
    }

    // Reception + Doctor can pull patient schedule
    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientAsync(patientId);
        return Ok(appointments);
    }

    // Doctor agenda view with adjustable window (defaults: past 7 days to next 7 days)
    [HttpGet("doctor/{doctorId}/range")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAppointmentsByDoctorRange(
        int doctorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.Today.AddDays(-7);
        var end = to?.Date ?? DateTime.Today.AddDays(7);

        if (end < start)
            return BadRequest("End date must be after start date.");

        var appointments = await _appointmentRepository.GetByDoctorAndRangeAsync(doctorId, start, end);
        return Ok(appointments);
    }

    // Create/update/delete limited to Admin/Reception
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
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
    [Authorize(Roles = "Admin,Receptionist")]
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
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var deleted = await _appointmentRepository.DeleteAsync(id);

        if (!deleted)
            return NotFound($"Appointment with ID {id} not found.");

        return Ok(new { Message = "Appointment deleted" });
    }
}
