using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IConsultationRepository
    {
        Task<Consultation> AddConsultationAsync(ConsultationRequestDTO request);
        Task<IEnumerable<PatientHistoryDTO>> GetPatientHistoryAsync(int patientId);
        Task<IEnumerable<PatientHistoryDetailDTO>> GetPatientHistoryWithDetailsAsync(int patientId);
        Task<Consultation?> GetByIdAsync(int id);
        Task<Consultation?> GetByIdWithDetailsAsync(int id);
        Task<Consultation?> GetByAppointmentIdAsync(int appointmentId);

        Task<bool> UpdateConsultationAsync(int id, ConsultationRequestDTO request);
        Task<bool> DeleteConsultationAsync(int id);
    }
}
