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

        public async Task<PagedResult<Patient>> GetAllAsync(int pageNumber = 1, int pageSize = 10, PatientSortBy sortBy = PatientSortBy.MRN)
        {
            var query = _context.Patients
                .Include(p => p.Appointments)
                .AsNoTracking()
                .AsQueryable();

            // Apply sorting
            query = sortBy switch
            {
                PatientSortBy.MRN => query.OrderBy(p => p.MMRNumber),
                PatientSortBy.Name => query.OrderBy(p => p.FullName),
                PatientSortBy.LastAppointment => query.OrderByDescending(p => 
                    p.Appointments.Any() ? p.Appointments.Max(a => a.AppointmentDate) : DateTime.MinValue),
                _ => query.OrderBy(p => p.MMRNumber)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Patient>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            // Auto-generate unique MRN number if not provided or empty
            if (string.IsNullOrWhiteSpace(patient.MMRNumber))
            {
                patient.MMRNumber = await GenerateUniqueMRNAsync();
            }
            else
            {
                // Validate that provided MRN is unique
                var existingPatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.MMRNumber == patient.MMRNumber);
                if (existingPatient != null)
                {
                    throw new InvalidOperationException($"MRN number {patient.MMRNumber} already exists. Please use a different MRN or leave it empty to auto-generate.");
                }
            }

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        private async Task<string> GenerateUniqueMRNAsync()
        {
            string mrn;
            bool isUnique = false;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                // Generate MRN in format: MRN + 6-digit number (e.g., MRN000001)
                // Get the highest existing MRN number
                var lastMRN = await _context.Patients
                    .Where(p => p.MMRNumber.StartsWith("MRN") && p.MMRNumber.Length == 9)
                    .OrderByDescending(p => p.MMRNumber)
                    .Select(p => p.MMRNumber)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastMRN))
                {
                    // Extract the number part (after "MRN")
                    var numberPart = lastMRN.Substring(3);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                // Format as MRN + 6-digit number with leading zeros
                mrn = $"MRN{nextNumber:D6}";

                // Check if this MRN already exists (in case of gaps or manual entries)
                isUnique = !await _context.Patients.AnyAsync(p => p.MMRNumber == mrn);
                attempts++;

                if (!isUnique && attempts < maxAttempts)
                {
                    // If not unique, try next number
                    continue;
                }
            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                throw new InvalidOperationException("Unable to generate a unique MRN number after multiple attempts. Please try again.");
            }

            return mrn;
        }

        public async Task<Patient?> UpdateAsync(int id, Patient patient)
        {
            var existing = await _context.Patients.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            // Prevent MRN from being changed - it's a unique identifier
            // If MRN is being changed, validate it's unique
            if (!string.IsNullOrWhiteSpace(patient.MMRNumber) && 
                patient.MMRNumber != existing.MMRNumber)
            {
                var duplicatePatient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.MMRNumber == patient.MMRNumber && p.PatientId != id);
                if (duplicatePatient != null)
                {
                    throw new InvalidOperationException($"MRN number {patient.MMRNumber} already exists. MRN cannot be changed to a duplicate value.");
                }
                existing.MMRNumber = patient.MMRNumber;
            }
            // If MRN is empty or null, keep the existing one
            // (Don't allow clearing MRN)

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
        public async Task<PagedResult<Patient>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10, PatientSortBy sortBy = PatientSortBy.MRN)
        {
            var query = _context.Patients
                .Include(p => p.Appointments)
                .AsNoTracking()
                .AsQueryable();

            // Unified search - search across MRN, name, and phone
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p => 
                    p.MMRNumber.ToLower().Contains(searchLower) ||
                    p.FullName.ToLower().Contains(searchLower) ||
                    p.Phone.Contains(searchTerm));
            }

            // Apply sorting
            query = sortBy switch
            {
                PatientSortBy.MRN => query.OrderBy(p => p.MMRNumber),
                PatientSortBy.Name => query.OrderBy(p => p.FullName),
                PatientSortBy.LastAppointment => query.OrderByDescending(p => 
                    p.Appointments.Any() ? p.Appointments.Max(a => a.AppointmentDate) : DateTime.MinValue),
                _ => query.OrderBy(p => p.MMRNumber)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Patient>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

    }
}

