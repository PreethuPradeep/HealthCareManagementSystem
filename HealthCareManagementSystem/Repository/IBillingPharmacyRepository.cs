using HealthCareManagementSystem.Models.Pharm;

namespace HealthCareManagementSystem.Repository
{
    public interface IBillingPharmacyRepository
    {
        Task<PharmacyBill> CreateBillAsync(PharmacyBill bill, List<PharmacyBillItem> items);
        Task<PharmacyBill?> GetBillByIdAsync(int billId);
    }
}
