using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models.Pharm
{
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

    
}
