using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date);
        Task<IEnumerable<Appointment>> GetByDoctorAndRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
        Task<Appointment> AddAsync(Appointment appointment);
        Task<Appointment?> UpdateAsync(int id, Appointment appointment);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Appointment>> GetPendingByDoctorAndDateAsync(int doctorId, DateTime date);
    }
}

