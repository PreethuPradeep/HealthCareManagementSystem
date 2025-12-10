using HealthCare.Database;
using HealthCareManagementSystem.Models.Pharm;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class BillingPharmacyRepository : IBillingPharmacyRepository
    {
        private readonly HealthCareDbContext _context;

        public BillingPharmacyRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<PharmacyBill> CreateBillAsync(PharmacyBill bill, List<PharmacyBillItem> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Calculate total
                bill.Total = items.Sum(i => i.LineTotal);

                _context.PharmacyBills.Add(bill);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    item.PharmacyBillId = bill.PharmacyBillId;

                    // Deduct stock
                    var med = await _context.Medicines.FindAsync(item.MedicineId);
                    if (med == null || med.StockQuantity < item.Quantity)
                        throw new Exception("Invalid stock condition detected during billing.");

                    med.StockQuantity -= item.Quantity;

                    _context.PharmacyBillItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return bill;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PharmacyBill?> GetBillByIdAsync(int billId)
        {
            return await _context.PharmacyBills
                .Include(b => b.Items)
                .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(b => b.PharmacyBillId == billId);
        }
    }
}
