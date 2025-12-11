using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IBillingRepository
    {
        Task<IEnumerable<Billing>> GetAllAsync();
        Task<Billing?> GetByIdAsync(int id);
        Task<IEnumerable<Billing>> GetByPatientAsync(int patientId);
        Task<Billing?> GetByAppointmentIdAsync(int appointmentId);
        Task<Billing> AddAsync(Billing billing);
        Task<Billing?> UpdateAsync(int id, Billing billing);
        Task<bool> DeleteAsync(int id);
    }
}
