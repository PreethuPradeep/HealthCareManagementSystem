using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models.Pharm
{
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }


        // Navigation properties
        public ICollection<PharmacyBillItem> BillItems { get; set; } = new List<PharmacyBillItem>();
        public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        public int StockQuantity { get; internal set; }
    }


}
