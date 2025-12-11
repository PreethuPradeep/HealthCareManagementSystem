using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HealthCareManagementSystem.Repository
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly HealthCareDbContext _context;

        public DoctorScheduleRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == doctorId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int scheduleId)
        {
            return await _context.DoctorSchedules.FindAsync(scheduleId);
        }

        public async Task<DoctorSchedule> AddAsync(DoctorSchedule schedule)
        {
            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<DoctorSchedule?> UpdateAsync(int id, DoctorSchedule schedule)
        {
            var existing = await _context.DoctorSchedules.FindAsync(id);
            if (existing == null)
                return null;

            existing.DayOfWeek = schedule.DayOfWeek;
            existing.StartTime = schedule.StartTime;
            existing.EndTime = schedule.EndTime;
            existing.IsActive = schedule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null)
                return false;

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                // Convert DateTime.DayOfWeek enum to string (Monday, Tuesday, etc.)
                var dayOfWeek = date.DayOfWeek.ToString();
                var schedules = await _context.DoctorSchedules
                    .AsNoTracking()
                    .Where(s => s.DoctorId == doctorId && 
                               s.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && 
                               s.IsActive)
                    .ToListAsync();

                if (!schedules.Any())
                    return Enumerable.Empty<string>();

                var availableSlots = new List<string>();
                var existingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.DoctorId == doctorId && 
                               a.AppointmentDate.Date == date.Date &&
                               a.IsActive)
                    .Select(a => a.TimeSlot)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    // Try to parse times, skip if invalid
                    if (string.IsNullOrWhiteSpace(schedule.StartTime) || string.IsNullOrWhiteSpace(schedule.EndTime))
                        continue;

                    if (!TimeSpan.TryParse(schedule.StartTime, out var startTime) ||
                        !TimeSpan.TryParse(schedule.EndTime, out var endTime))
                        continue;

                    var slotDuration = TimeSpan.FromMinutes(15); // Each token is 15 minutes

                    var currentTime = startTime;
                    while (currentTime < endTime)
                    {
                        var timeSlot = $"{currentTime.Hours:D2}:{currentTime.Minutes:D2}";
                        
                        // Check if this slot is already booked
                        if (!existingAppointments.Contains(timeSlot))
                        {
                            availableSlots.Add(timeSlot);
                        }

                        currentTime = currentTime.Add(slotDuration);
                    }
                }

                return availableSlots.OrderBy(t => t);
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logger here)
                // For now, return empty list on error
                return Enumerable.Empty<string>();
            }
        }
    }
}

