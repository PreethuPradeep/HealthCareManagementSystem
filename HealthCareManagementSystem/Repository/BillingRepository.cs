using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class BillingRepository : IBillingRepository
    {
        private readonly HealthCareDbContext _context;

        public BillingRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Billing>> GetAllAsync()
        {
            return await _context.Billings
                .AsNoTracking()
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<Billing?> GetByIdAsync(int id)
        {
            return await _context.Billings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BillingId == id);
        }

        public async Task<IEnumerable<Billing>> GetByPatientAsync(int patientId)
        {
            return await _context.Billings
                .Where(b => b.PatientId == patientId)
                .AsNoTracking()
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<Billing> AddAsync(Billing billing)
        {
            billing.CreatedAt = DateTime.UtcNow;
            billing.BillingDate = DateTime.UtcNow;

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();
            return billing;
        }

        public async Task<Billing?> UpdateAsync(int id, Billing billing)
        {
            var existing = await _context.Billings.FindAsync(id);
            if (existing == null)
                return null;

            existing.PatientId = billing.PatientId;
            existing.AppointmentId = billing.AppointmentId;

            existing.Amount = billing.Amount;
            existing.Description = billing.Description;
            existing.Status = billing.Status;

            existing.DueDate = billing.DueDate;
            existing.PaidDate = billing.PaidDate;
            existing.PaymentMethod = billing.PaymentMethod;

            existing.PatientName = billing.PatientName;
            existing.PatientPhone = billing.PatientPhone;
            existing.PatientAddress = billing.PatientAddress;
            existing.DoctorName = billing.DoctorName;
            existing.Notes = billing.Notes;

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null)
                return false;

            _context.Billings.Remove(billing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
