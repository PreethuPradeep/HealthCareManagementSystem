using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllActiveStaffAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<int> AddStaffAsync(CreateStaffDto dto);
        Task<int> UpdateStaffAsync(ApplicationUser user);
        Task<int> DeactivateStaffAsync(string userId);
        Task<ApplicationUser?> AuthenticateAsync(string email, string password);
    }
}
