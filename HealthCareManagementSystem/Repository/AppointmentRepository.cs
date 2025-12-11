using HealthCare.Database;
using HealthCareManagementSystem.Models;
using HealthCare.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly HealthCareDbContext _context;

        public AppointmentRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        // Listing methods
        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TokenNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.IsActive && a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.TokenNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date && 
                           a.IsActive)
                .OrderBy(a => a.TokenNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorAndRangeAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date >= startDate.Date &&
                           a.AppointmentDate.Date <= endDate.Date &&
                           a.IsActive)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TokenNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.PatientId == patientId && a.IsActive)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetPendingByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.IsVisited == false &&
                           a.IsActive)
                .OrderBy(a => a.TokenNo)
                .ToListAsync();
        }

        // Detail methods
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.IsActive);
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.IsActive);
        }

        // CRUD operations
        public async Task<Appointment> CreateAsync(CreateAppointmentRequestDTO request, string patientMMR)
        {
            // Validate time slot availability
            if (!await IsTimeSlotAvailableAsync(request.DoctorId, request.AppointmentDate, request.TimeSlot))
            {
                throw new InvalidOperationException($"Time slot {request.TimeSlot} is already booked for this doctor on {request.AppointmentDate:yyyy-MM-dd}");
            }

            // Get patient information
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
            {
                throw new InvalidOperationException($"Patient with ID {request.PatientId} not found.");
            }

            // Get doctor information
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);
            if (doctor == null)
            {
                throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found.");
            }

            // Generate token number
            var tokenNo = await GetNextTokenNumberAsync(request.DoctorId, request.AppointmentDate);

            // Set consultation fee from doctor's user profile or doctor fee
            decimal? consultationFee = null;
            if (doctor.User != null && doctor.User.ConsultationFee.HasValue && doctor.User.ConsultationFee.Value > 0)
            {
                consultationFee = doctor.User.ConsultationFee.Value;
            }
            else if (doctor.Fee > 0)
            {
                consultationFee = doctor.Fee;
            }

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                PatientMMR = patientMMR,
                PatientName = patient.FullName,
                PatientPhone = patient.Phone,
                PatientAddress = patient.Address,
                DoctorId = request.DoctorId,
                DoctorName = doctor.User?.FullName ?? string.Empty,
                AppointmentDate = request.AppointmentDate.Date,
                TimeSlot = request.TimeSlot,
                ConsultationType = request.ConsultationType,
                ConsultationFee = consultationFee, // Set consultation fee when creating appointment
                Reason = request.Reason,
                TokenNo = tokenNo,
                IsVisited = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            
            // Reload with includes
            return await GetByIdWithDetailsAsync(appointment.AppointmentId) ?? appointment;
        }

        public async Task<Appointment?> UpdateAsync(int id, UpdateAppointmentRequestDTO request)
        {
            var existing = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.IsActive);

            if (existing == null)
            {
                return null;
            }

            // If date, doctor, or time slot changed, validate availability
            if (existing.DoctorId != request.DoctorId || 
                existing.AppointmentDate.Date != request.AppointmentDate.Date ||
                existing.TimeSlot != request.TimeSlot)
            {
                if (!await IsTimeSlotAvailableAsync(request.DoctorId, request.AppointmentDate, request.TimeSlot, id))
                {
                    throw new InvalidOperationException($"Time slot {request.TimeSlot} is already booked for this doctor on {request.AppointmentDate:yyyy-MM-dd}");
                }

                // If date or doctor changed, regenerate token number
                if (existing.DoctorId != request.DoctorId || 
                    existing.AppointmentDate.Date != request.AppointmentDate.Date)
                {
                    existing.TokenNo = await GetNextTokenNumberAsync(request.DoctorId, request.AppointmentDate);
                }
            }

            // Update patient information if patient changed
            if (existing.PatientId != request.PatientId)
            {
                var patient = await _context.Patients.FindAsync(request.PatientId);
                if (patient == null)
                {
                    throw new InvalidOperationException($"Patient with ID {request.PatientId} not found.");
                }
                existing.PatientId = request.PatientId;
                existing.PatientName = patient.FullName;
                existing.PatientPhone = patient.Phone;
                existing.PatientAddress = patient.Address;
            }

            // Update doctor information if doctor changed
            if (existing.DoctorId != request.DoctorId)
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);
                if (doctor == null)
                {
                    throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found.");
                }
                existing.DoctorId = request.DoctorId;
                existing.DoctorName = doctor.User?.FullName ?? string.Empty;
            }

            // Update appointment fields
            existing.AppointmentDate = request.AppointmentDate.Date;
            existing.TimeSlot = request.TimeSlot;
            existing.ConsultationType = request.ConsultationType;
            existing.Status = request.Status;
            existing.Reason = request.Reason;
            existing.IsVisited = request.Status == "Completed" || request.Status == "Visited";
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Reload with includes
            return await GetByIdWithDetailsAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null || !appointment.IsActive)
            {
                return false;
            }

            // Soft delete
            appointment.IsActive = false;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Validation methods
        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, string timeSlot, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.TimeSlot == timeSlot &&
                           a.IsActive);

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<int> GetNextTokenNumberAsync(int doctorId, DateTime date)
        {
            var currentMax = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.IsActive)
                .MaxAsync(a => (int?)a.TokenNo) ?? 0;

            return currentMax + 1;
        }
    }
}
