using HealthCareManagementSystem.Models;

namespace HealthCareManagementSystem.Repository
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllActiveStaffAsync();
        Task<int> AddStaffAsync(ApplicationUser user);
        Task<int> UpdateStaffAsync(ApplicationUser user);
        Task<int> DeactivateStaffAsync(string userId);
        Task<ApplicationUser?> AuthenticateAsync(string email, string password);
        Task<IEnumerable<ApplicationUser>> GetDoctorsBySpecializationAsync(int specializationId);
        Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName);


    }
}
