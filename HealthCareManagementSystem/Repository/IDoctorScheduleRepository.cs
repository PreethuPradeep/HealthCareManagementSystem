using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IDoctorScheduleRepository
    {
        Task<IEnumerable<DoctorSchedule>> GetByDoctorIdAsync(int doctorId);
        Task<DoctorSchedule?> GetByIdAsync(int scheduleId);
        Task<DoctorSchedule> AddAsync(DoctorSchedule schedule);
        Task<DoctorSchedule?> UpdateAsync(int id, DoctorSchedule schedule);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<string>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);
    }
}

