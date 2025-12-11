using HealthCareManagementSystem.Models;
using HealthCare.Models.DTOs;

namespace HealthCareManagementSystem.Repository
{
    public interface IAppointmentRepository
    {
        // Listing methods
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
        Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(int doctorId, DateTime date);
        Task<IEnumerable<Appointment>> GetByDoctorAndRangeAsync(int doctorId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetPendingByDoctorAndDateAsync(int doctorId, DateTime date);
        
        // Detail methods
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment?> GetByIdWithDetailsAsync(int id);
        
        // CRUD operations
        Task<Appointment> CreateAsync(CreateAppointmentRequestDTO request, string patientMMR);
        Task<Appointment?> UpdateAsync(int id, UpdateAppointmentRequestDTO request);
        Task<bool> DeleteAsync(int id);
        
        // Validation methods
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, string timeSlot, int? excludeAppointmentId = null);
        Task<int> GetNextTokenNumberAsync(int doctorId, DateTime date);
    }
}

