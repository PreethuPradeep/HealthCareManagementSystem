using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } // Identity uses String IDs by default
        public ApplicationUser? User { get; set; }

        [ForeignKey("Specialization")]
        public int SpecializationId { get; set; }
        public Specialization? Specialization { get; set; }

        public decimal Fee { get; set; }
        public bool IsActive { get; set; }

        //navigation
        //these are available for each doctors
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Consultation>? Consultations { get; set; }
        public ICollection<LabTestRequest>? LabTestsOrdered { get; set; }
        public ICollection<Prescription>? Prescriptions { get; set; }
    }

    // Pharmacy Models

}
