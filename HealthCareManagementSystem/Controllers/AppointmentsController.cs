using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentRepository appointmentRepository,
        ILogger<AppointmentsController> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all appointments (Admin, Receptionist)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetAllAppointments([FromQuery] DateTime? date)
    {
        try
        {
            IEnumerable<Appointment> appointments;
            
            if (date.HasValue)
            {
                appointments = await _appointmentRepository.GetByDateAsync(date.Value);
            }
            else
            {
                appointments = await _appointmentRepository.GetAllAsync();
            }

            var response = appointments.Select(MapToDetailResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments");
            return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get appointment by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetAppointmentById(int id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);

            if (appointment == null)
            {
                return NotFound(new { Message = $"Appointment with ID {id} not found." });
            }

            var response = MapToDetailResponse(appointment);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointment {AppointmentId}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the appointment", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get appointments by patient ID
    /// </summary>
    [HttpGet("patient/{patientId}")]
    [Authorize(Roles = "Admin,Receptionist,Doctor")]
    public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
    {
        try
        {
            if (patientId <= 0)
            {
                return BadRequest(new { Message = "Invalid patient ID" });
            }

            var appointments = await _appointmentRepository.GetByPatientAsync(patientId);
            var response = appointments.Select(MapToDetailResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for patient {PatientId}", patientId);
            return StatusCode(500, new { Message = "An error occurred while retrieving patient appointments", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get appointments by doctor and date
    /// </summary>
    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAppointmentsByDoctor(int doctorId, [FromQuery] DateTime? date)
    {
        try
        {
            if (doctorId <= 0)
            {
                return BadRequest(new { Message = "Invalid doctor ID" });
            }

            var targetDate = date ?? DateTime.Today;
            var appointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, targetDate);
            var response = appointments.Select(MapToDetailResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId}", doctorId);
            return StatusCode(500, new { Message = "An error occurred while retrieving doctor appointments", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get pending appointments by doctor and date
    /// </summary>
    [HttpGet("doctor/{doctorId}/pending")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetPendingAppointments(int doctorId, [FromQuery] DateTime? date)
    {
        try
        {
            if (doctorId <= 0)
            {
                return BadRequest(new { Message = "Invalid doctor ID" });
            }

            var targetDate = date ?? DateTime.Today;
            var appointments = await _appointmentRepository.GetPendingByDoctorAndDateAsync(doctorId, targetDate);
            var response = appointments.Select(MapToDetailResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending appointments for doctor {DoctorId}", doctorId);
            return StatusCode(500, new { Message = "An error occurred while retrieving pending appointments", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get appointments by doctor within date range
    /// </summary>
    [HttpGet("doctor/{doctorId}/range")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAppointmentsByDoctorRange(
        int doctorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            if (doctorId <= 0)
            {
                return BadRequest(new { Message = "Invalid doctor ID" });
            }

            var start = from?.Date ?? DateTime.Today.AddDays(-7);
            var end = to?.Date ?? DateTime.Today.AddDays(7);

            if (end < start)
            {
                return BadRequest(new { Message = "End date must be after start date." });
            }

            var appointments = await _appointmentRepository.GetByDoctorAndRangeAsync(doctorId, start, end);
            var response = appointments.Select(MapToDetailResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appointments for doctor {DoctorId} in range", doctorId);
            return StatusCode(500, new { Message = "An error occurred while retrieving appointments", Error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDTO request, [FromQuery] string? patientMMR)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data", Errors = ModelState });
            }

            // Validate appointment date is not in the past
            if (request.AppointmentDate.Date < DateTime.Today)
            {
                return BadRequest(new { Message = "Appointment date cannot be in the past" });
            }

            // Get patient MMR if not provided
            if (string.IsNullOrWhiteSpace(patientMMR))
            {
                // Patient MMR should be provided by the frontend, but if not, we'll fetch it
                // For now, we'll require it in the request or throw an error
                return BadRequest(new { Message = "Patient MMR is required. Please provide it in the query parameter or ensure the patient exists." });
            }

            var appointment = await _appointmentRepository.CreateAsync(request, patientMMR);
            var response = MapToDetailResponse(appointment);

            return CreatedAtAction(
                nameof(GetAppointmentById),
                new { id = appointment.AppointmentId },
                response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating appointment");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, new { Message = "An error occurred while creating the appointment", Error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing appointment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequestDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid request data", Errors = ModelState });
            }

            if (id <= 0)
            {
                return BadRequest(new { Message = "Invalid appointment ID" });
            }

            // Validate appointment date is not in the past
            if (request.AppointmentDate.Date < DateTime.Today)
            {
                return BadRequest(new { Message = "Appointment date cannot be in the past" });
            }

            var appointment = await _appointmentRepository.UpdateAsync(id, request);

            if (appointment == null)
            {
                return NotFound(new { Message = $"Appointment with ID {id} not found." });
            }

            var response = MapToDetailResponse(appointment);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating appointment {AppointmentId}", id);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {AppointmentId}", id);
            return StatusCode(500, new { Message = "An error occurred while updating the appointment", Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete (soft delete) an appointment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "Invalid appointment ID" });
            }

            var deleted = await _appointmentRepository.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { Message = $"Appointment with ID {id} not found." });
            }

            return Ok(new { Message = "Appointment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the appointment", Error = ex.Message });
        }
    }

    // Helper method to map Appointment entity to DTO
    private AppointmentDetailResponseDTO MapToDetailResponse(Appointment appointment)
    {
        return new AppointmentDetailResponseDTO
        {
            AppointmentId = appointment.AppointmentId,
            TokenNo = appointment.TokenNo,
            PatientId = appointment.PatientId ?? 0,
            Patient = appointment.Patient != null ? new PatientInfoDTO
            {
                PatientId = appointment.Patient.PatientId,
                FullName = appointment.Patient.FullName,
                MMRNumber = appointment.Patient.MMRNumber,
                DOB = appointment.Patient.DOB,
                Gender = appointment.Patient.Gender,
                Phone = appointment.Patient.Phone,
                Email = appointment.Patient.Email,
                Address = appointment.Patient.Address
            } : null,
            DoctorId = appointment.DoctorId,
            Doctor = appointment.Doctor != null ? new DoctorInfoDTO
            {
                DoctorId = appointment.Doctor.DoctorId,
                FullName = appointment.Doctor.User?.FullName ?? appointment.DoctorName ?? string.Empty,
                Specialization = appointment.Doctor.Specialization?.SpecializationName
            } : null,
            AppointmentDate = appointment.AppointmentDate,
            TimeSlot = appointment.TimeSlot,
            Status = appointment.Status,
            Reason = appointment.Reason,
            ConsultationType = appointment.ConsultationType,
            ConsultationFee = appointment.ConsultationFee,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}
