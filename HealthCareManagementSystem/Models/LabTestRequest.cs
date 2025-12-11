using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class LabTestRequest
    {
        [Key]
        public int LabTestRequestId { get; set; }

        public int ConsultationId { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        [Required]
        public string TestName { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        public string? Result { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
        public Consultation Consultation { get; set; }
    }
}
