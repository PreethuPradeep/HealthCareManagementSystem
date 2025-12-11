using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class UserSqlServerRepositoryImpl : IUserRepository
    {
        private readonly HealthCareDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSqlServerRepositoryImpl(HealthCareDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllActiveStaffAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive == true)
                .Include(u => u.Role)
                .Include(u => u.Specialization)
                .ToListAsync();
        }

        public async Task<int> AddStaffAsync(ApplicationUser user)
        {
            _context.Users.Add(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateStaffAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeactivateStaffAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return 0;

            user.IsActive = false;
            _context.Users.Update(user);

            return await _context.SaveChangesAsync();
        }

        //Authenticate method using Identity
        public async Task<ApplicationUser?> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);

            if (user == null)
                return null;

            var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!isValidPassword)
                return null;

            return user;
        }

        public Task<IEnumerable<ApplicationUser>> GetDoctorsBySpecializationAsync(int specializationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
