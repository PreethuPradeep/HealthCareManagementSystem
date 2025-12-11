using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public enum PatientSortBy
    {
        MRN,
        Name,
        LastAppointment
    }

    public interface IPatientRepository
    {
        Task<PagedResult<Patient>> GetAllAsync(int pageNumber = 1, int pageSize = 10, PatientSortBy sortBy = PatientSortBy.MRN);
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient> AddAsync(Patient patient);
        Task<Patient?> UpdateAsync(int id, Patient patient);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<Patient>> SearchAsync(string? searchTerm, int pageNumber = 1, int pageSize = 10, PatientSortBy sortBy = PatientSortBy.MRN);
    }
}

