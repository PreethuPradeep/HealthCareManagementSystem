using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models.Pharm
{
    public class PrescriptionItem
    {
        [Key]
        public int PrescriptionItemId { get; set; }

        public int PrescriptionId { get; set; }

        public int MedicineId { get; set; }

        [StringLength(50)]
        public string MealTime { get; set; } = string.Empty;

        public int MorningDose { get; set; }

        public int NoonDose { get; set; }

        public int EveningDose { get; set; }

        public int? Quantity { get; set; }

        public int? Dosage { get; set; }

        public int DurationInDays { get; set; }

        // Navigation properties
        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }

        [ForeignKey("MedicineId")]
        public Medicine? Medicine { get; set; }
    }

    
}
