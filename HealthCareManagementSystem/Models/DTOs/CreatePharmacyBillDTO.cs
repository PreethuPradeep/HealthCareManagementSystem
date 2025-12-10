namespace HealthCare.Models.DTOs
{
    public class CreatePharmacyBillItemDTO
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreatePharmacyBillDTO
    {
        public List<CreatePharmacyBillItemDTO> Items { get; set; } = new();
    }
}

