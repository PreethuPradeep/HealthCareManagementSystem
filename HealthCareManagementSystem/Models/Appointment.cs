using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthCareManagementSystem.Models
{
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentId { get; set; }

        public int TokenNo { get; set; } // Auto-generated per day

        [Required(ErrorMessage = "Patient MMR is required")]
        [StringLength(10)]
        [RegularExpression(@"^MMR\d{6}$", ErrorMessage = "Patient MMR must be in format MMR000000")]
        public string PatientMMR { get; set; } = string.Empty;

        public int? PatientId { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Patient Name can only contain letters and spaces")]
        public string? PatientName { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Patient Phone must be exactly 10 digits")]
        public string? PatientPhone { get; set; }

        [StringLength(250)]
        public string? PatientAddress { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Doctor ID must be a positive number")]
        public int DoctorId { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\.]+$", ErrorMessage = "Doctor Name can only contain letters, spaces, and dots")]
        public string? DoctorName { get; set; }

        [Required(ErrorMessage = "Appointment Date is required")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time Slot is required")]
        [StringLength(50)]
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9](\s?(AM|PM|am|pm))?$", ErrorMessage = "Time Slot must be in HH:mm or HH:mm AM/PM format")]
        public string TimeSlot { get; set; } = string.Empty;

        [Required(ErrorMessage = "Consultation Type is required")]
        [StringLength(100)]
        public string ConsultationType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99, ErrorMessage = "Consultation Fee must be between 0 and 999999.99")]
        public decimal? ConsultationFee { get; set; }

        public bool IsVisited { get; set; }

        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string Status => IsVisited ? "Visited" : "Pending";

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}

