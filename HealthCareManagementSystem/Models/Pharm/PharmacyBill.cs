using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models.Pharm
{
    public class PharmacyBill
    {
        [Key]
        public int PharmacyBillId { get; set; }

        public DateTime BillDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<PharmacyBillItem> Items { get; set; } = new List<PharmacyBillItem>();
    }
}
