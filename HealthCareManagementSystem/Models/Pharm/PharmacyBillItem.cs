using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models.Pharm
{
    public class PharmacyBillItem
    {
        [Key]
        public int PharmacyBillItemId { get; set; }

        public int PharmacyBillId { get; set; }
        public PharmacyBill? PharmacyBill { get; set; }

        public int MedicineId { get; set; }
        public Medicine? Medicine { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}
