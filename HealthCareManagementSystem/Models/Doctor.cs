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
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? BatchNo { get; set; }
        
        [StringLength(100)]
        public string? Manufacturer { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        public int Stock { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<PharmacyBillItem> BillItems { get; set; } = new List<PharmacyBillItem>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    }

    public class PharmacyBill
    {
        [Key]
        public int PharmacyBillId { get; set; }
        public int? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Tax { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }
        
        // Navigation property
        public ICollection<PharmacyBillItem> Items { get; set; } = new List<PharmacyBillItem>();
    }

    public class PharmacyBillItem
    {
        [Key]
        public int PharmacyBillItemId { get; set; }
        
        public int PharmacyBillId { get; set; }
        
        public int MedicineId { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }
        
        // Navigation properties
        [ForeignKey("PharmacyBillId")]
        public PharmacyBill? PharmacyBill { get; set; }
        
        [ForeignKey("MedicineId")]
        public Medicine? Medicine { get; set; }
    }

    public class StockTransaction
    {
        [Key]
        public int StockTransactionId { get; set; }
        
        public int MedicineId { get; set; }
        
        public int QuantityChange { get; set; }
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Sale, Purchase, Adjustment
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        [ForeignKey("MedicineId")]
        public Medicine? Medicine { get; set; }
    }

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

    public class MedicinePrescriptionDetails
    {
        [Key]
        public int MedicinePrescriptionDetailsId { get; set; }
        
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
