using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class Consultation
    {
        [Key]
        public int ConsultationId { get; set; }

        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }

        public DateTime DateBooked { get; set; } = DateTime.UtcNow;

        // SOAP Notes
        public string? ChiefComplaint { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorNotes { get; set; }

        public string? NextActions { get; set; }
        public DateTime? FollowUpDate { get; set; }

        // Navigation Properties
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
        public ICollection<Prescription>? Prescriptions { get; set; }
        public ICollection<LabTestRequest>? LabTests { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

    }
}