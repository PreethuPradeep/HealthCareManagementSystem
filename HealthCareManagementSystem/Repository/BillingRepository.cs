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
                .Include(b => b.Appointment)
                    .ThenInclude(a => a!.Doctor)
                        .ThenInclude(d => d.User)
                .Include(b => b.Patient)
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

        public async Task<Billing?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Billings
                .AsNoTracking()
                .Include(b => b.Appointment)
                    .ThenInclude(a => a!.Doctor)
                        .ThenInclude(d => d.User)
                .Include(b => b.Patient)
                .FirstOrDefaultAsync(b => b.AppointmentId == appointmentId);
        }

        public async Task<Billing> AddAsync(Billing billing)
        {
            billing.CreatedAt = DateTime.UtcNow;
            billing.BillingDate = DateTime.UtcNow;
            
            // Ensure Status is set
            if (string.IsNullOrWhiteSpace(billing.Status))
            {
                billing.Status = "Pending";
            }

            // If appointment ID is provided, populate patient and doctor information
            if (billing.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == billing.AppointmentId.Value && a.IsActive);

                if (appointment != null)
                {
                    // Populate patient information
                    if (appointment.Patient != null)
                    {
                        billing.PatientId = appointment.Patient.PatientId;
                        billing.PatientName = appointment.Patient.FullName;
                        billing.PatientPhone = appointment.Patient.Phone;
                        billing.PatientAddress = appointment.Patient.Address;
                    }

                    // Populate doctor information
                    if (appointment.Doctor != null)
                    {
                        billing.DoctorName = appointment.Doctor.User?.FullName ?? appointment.DoctorName ?? string.Empty;
                    }

                    // Set consultation fee from appointment or doctor
                    if (billing.Amount == 0)
                    {
                        // Priority order:
                        // 1. Appointment.ConsultationFee (if set)
                        // 2. Doctor.User.ConsultationFee (from user profile - this is what user sets)
                        // 3. Doctor.Fee (from doctor table)
                        if (appointment.ConsultationFee.HasValue && appointment.ConsultationFee.Value > 0)
                        {
                            billing.Amount = appointment.ConsultationFee.Value;
                        }
                        else if (appointment.Doctor != null)
                        {
                            // If User is not loaded, reload the doctor with user
                            if (appointment.Doctor.User == null && !string.IsNullOrEmpty(appointment.Doctor.UserId))
                            {
                                appointment.Doctor.User = await _context.Set<ApplicationUser>()
                                    .FirstOrDefaultAsync(u => u.Id == appointment.Doctor.UserId);
                            }
                            
                            // Prioritize User.ConsultationFee over Doctor.Fee
                            if (appointment.Doctor.User != null && 
                                appointment.Doctor.User.ConsultationFee.HasValue && 
                                appointment.Doctor.User.ConsultationFee.Value > 0)
                            {
                                billing.Amount = appointment.Doctor.User.ConsultationFee.Value;
                            }
                            else if (appointment.Doctor.Fee > 0)
                            {
                                billing.Amount = appointment.Doctor.Fee;
                            }
                        }
                        
                        // If still 0, set a default minimum amount to pass validation
                        if (billing.Amount == 0)
                        {
                            billing.Amount = 0.01m; // Minimum amount required by validation
                        }
                    }

                    // Set description if not provided
                    if (string.IsNullOrWhiteSpace(billing.Description))
                    {
                        billing.Description = $"Consultation fee for appointment on {appointment.AppointmentDate:yyyy-MM-dd}";
                    }
                }
                else
                {
                    // Appointment not found - this is an error condition
                    throw new InvalidOperationException($"Appointment with ID {billing.AppointmentId.Value} not found or is inactive.");
                }
            }
            else
            {
                // If no appointment ID, ensure PatientId is valid
                if (billing.PatientId <= 0)
                {
                    throw new InvalidOperationException("Patient ID is required when creating billing without an appointment.");
                }
                
                // Ensure amount is valid
                if (billing.Amount <= 0)
                {
                    throw new InvalidOperationException("Amount must be greater than 0.");
                }
            }

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
