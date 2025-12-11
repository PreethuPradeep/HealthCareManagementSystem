using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class DoctorSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, etc.

        [Required]
        [StringLength(10)]
        public string StartTime { get; set; } = string.Empty; // Format: "HH:mm" (e.g., "09:00")

        [Required]
        [StringLength(10)]
        public string EndTime { get; set; } = string.Empty; // Format: "HH:mm" (e.g., "17:00")

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

