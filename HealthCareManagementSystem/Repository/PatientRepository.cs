using HealthCare.Database;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly HealthCareDbContext _context;

        public PatientRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.PatientId)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient?> UpdateAsync(int id, Patient patient)
        {
            var existing = await _context.Patients.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.MMRNumber = patient.MMRNumber;
            existing.FullName = patient.FullName;
            existing.Gender = patient.Gender;
            existing.Phone = patient.Phone;
            existing.Address = patient.Address;
            existing.DOB = patient.DOB;
            existing.Email = patient.Email;
            existing.Membership = patient.Membership;
            existing.MembershipId = patient.MembershipId;
            existing.IsActive = patient.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return false;
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Patient>> SearchAsync(string? mmr, string? name, string? phone)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(mmr))
                query = query.Where(p => p.MMRNumber.Contains(mmr));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.FullName.Contains(name));

            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(p => p.Phone.Contains(phone));

            return await query
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

    }
}

