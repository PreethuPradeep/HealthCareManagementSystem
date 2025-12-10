using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthCareManagementSystem.Models
{
    public class Billing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillingId { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive number")]
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 9999999.99, ErrorMessage = "Amount must be between 0.01 and 9999999.99")]
        public decimal Amount { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50)]
        [RegularExpression(@"^(Pending|Paid|Overdue|Cancelled)$", ErrorMessage = "Status must be Pending, Paid, Overdue, or Cancelled")]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime BillingDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Patient Name can only contain letters and spaces")]
        public string? PatientName { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Patient Phone must be exactly 10 digits")]
        public string? PatientPhone { get; set; }

        [StringLength(250)]
        public string? PatientAddress { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s\.]+$", ErrorMessage = "Doctor Name can only contain letters, spaces, and dots")]
        public string? DoctorName { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        [JsonIgnore]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("AppointmentId")]
        [JsonIgnore]
        public virtual Appointment? Appointment { get; set; }
        public string? CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }

    }
}

