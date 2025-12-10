using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int ConsultationId { get; set; }
        public string MedicineName { get; set; }
        [ForeignKey("ConsultationId")]
        public Consultation? Consultation { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
