using HealthCare.Database;
using HealthCareManagementSystem.Models;
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

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .AsNoTracking()
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TokenNo)
                .ToListAsync();
        }
        public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.TokenNo)
                .ToListAsync();
        }
        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments.FindAsync(id);
        }

        public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            // Generate a token number for the appointment date
            var currentMax = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == appointment.AppointmentDate.Date)
                .MaxAsync(a => (int?)a.TokenNo) ?? 0;

            appointment.TokenNo = currentMax + 1;
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment?> UpdateAsync(int id, Appointment appointment)
        {
            var existing = await _context.Appointments.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.PatientId = appointment.PatientId;
            existing.DoctorId = appointment.DoctorId;
            existing.AppointmentDate = appointment.AppointmentDate;
            existing.TimeSlot = appointment.TimeSlot;
            existing.ConsultationType = appointment.ConsultationType;
            existing.ConsultationFee = appointment.ConsultationFee;
            existing.IsVisited = appointment.IsVisited;
            existing.IsActive = appointment.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return false;
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Appointment>> GetPendingByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.IsVisited == false)
                .OrderBy(a => a.TokenNo)
                .ToListAsync();
        }

    }
}

