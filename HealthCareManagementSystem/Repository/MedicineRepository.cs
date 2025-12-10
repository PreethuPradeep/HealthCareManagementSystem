using HealthCare.Database;
using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly HealthCareDbContext _context;

        public MedicineRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Medicine>> GetAllAsync()
        {
            return await _context.Medicines.ToListAsync();
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            return await _context.Medicines.FindAsync(id);
        }

        public async Task<Medicine> AddAsync(Medicine medicine)
        {
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();
            return medicine;
        }

        public async Task<Medicine> UpdateAsync(Medicine medicine)
        {
            _context.Medicines.Update(medicine);
            await _context.SaveChangesAsync();
            return medicine;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var med = await _context.Medicines.FindAsync(id);
            if (med == null) return false;

            _context.Medicines.Remove(med);
            await _context.SaveChangesAsync();
            return true;
        }

        // 🔹 Check available stock
        public async Task<bool> CheckStockAsync(int medicineId, int requiredQty)
        {
            var med = await _context.Medicines.FindAsync(medicineId);
            if (med == null) return false;

            return med.Stock >= requiredQty;
        }

        // 🔹 Reduce stock after billing
        public async Task<bool> ReduceStockAsync(int medicineId, int quantity)
        {
            var med = await _context.Medicines.FindAsync(medicineId);
            if (med == null || med.Stock < quantity) return false;

            med.Stock -= quantity;

            _context.Medicines.Update(med);

            // Add a stock transaction
            _context.StockTransactions.Add(new StockTransaction
            {
                MedicineId = medicineId,
                QuantityChange = -quantity,
                Type = "Sale",
                Remarks = "Stock deducted during billing"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MedicineListDTO>> GetListAsync()
        {
            return await _context.Medicines
                .Select(m => new MedicineListDTO
                {
                    MedicineId = m.MedicineId,
                    Name = m.Name,
                    BatchNo = m.BatchNo,
                    Manufacturer = m.Manufacturer,
                    Stock = m.Stock,
                    UnitPrice = m.UnitPrice
                })
                .ToListAsync();
        }

        public async Task<MedicineDetailsDTO?> GetDetailsAsync(int id)
        {
            return await _context.Medicines
                .Where(m => m.MedicineId == id)
                .Select(m => new MedicineDetailsDTO
                {
                    MedicineId = m.MedicineId,
                    Name = m.Name,
                    BatchNo = m.BatchNo,
                    Manufacturer = m.Manufacturer,
                    ExpiryDate = m.ExpiryDate,
                    UnitPrice = m.UnitPrice,
                    Stock = m.Stock
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MedicineListDTO>> SearchAsync(string query)
        {
            query = query.ToLower();

            return await _context.Medicines
                .Where(m =>
                    m.Name.ToLower().Contains(query) ||
                    (m.Manufacturer ?? "").ToLower().Contains(query) ||
                    (m.BatchNo ?? "").ToLower().Contains(query))
                .Select(m => new MedicineListDTO
                {
                    MedicineId = m.MedicineId,
                    Name = m.Name,
                    BatchNo = m.BatchNo,
                    Manufacturer = m.Manufacturer,
                    Stock = m.Stock,
                    UnitPrice = m.UnitPrice
                })
                .ToListAsync();
        }

    }
}
