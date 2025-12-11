using System.ComponentModel.DataAnnotations;

namespace HealthCare.Models.DTOs
{
    // Request DTO for creating appointments
    public class CreateAppointmentRequestDTO
    {
        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Doctor ID must be a positive number")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Appointment Date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time Slot is required")]
        [StringLength(50)]
        public string TimeSlot { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string ConsultationType { get; set; } = "General";
    }

    // Request DTO for updating appointments
    public class UpdateAppointmentRequestDTO
    {
        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Doctor ID must be a positive number")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Appointment Date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time Slot is required")]
        [StringLength(50)]
        public string TimeSlot { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(100)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(100)]
        public string ConsultationType { get; set; } = "General";
    }

    // Response DTO for appointment listing
    public class AppointmentListResponseDTO
    {
        public int AppointmentId { get; set; }
        public int TokenNo { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientMMR { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string ConsultationType { get; set; } = "General";
        public DateTime CreatedAt { get; set; }
    }

    // Response DTO for appointment details
    public class AppointmentDetailResponseDTO
    {
        public int AppointmentId { get; set; }
        public int TokenNo { get; set; }
        public int PatientId { get; set; }
        public PatientInfoDTO? Patient { get; set; }
        public int DoctorId { get; set; }
        public DoctorInfoDTO? Doctor { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string ConsultationType { get; set; } = "General";
        public decimal? ConsultationFee { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Patient info for appointment details
    public class PatientInfoDTO
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MMRNumber { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
    }

    // Doctor info for appointment details
    public class DoctorInfoDTO
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
    }
}

