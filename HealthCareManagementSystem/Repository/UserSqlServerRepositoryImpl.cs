using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        public async Task<int> AddStaffAsync(CreateStaffDto model)
        {
            var identityUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                Gender = model.Gender,
                DateOfJoin = model.DateOfJoin,
                DateOfBirth = model.DateOfBirth,
                MobileNumber = model.MobileNumber,
                Address = model.Address,
                RoleId = model.RoleId,             // Your custom role
                SpecializationId = model.SpecializationId,
                ConsultationFee = model.ConsultationFee,
                IsActive = model.IsActive,
                EmailConfirmed = true
            };

            // Create user with Identity
            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            return 1;
        }

        public async Task<int> UpdateStaffAsync(ApplicationUser user)
        {
            // Load the existing entity from the database first
            var existingUser = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Specialization)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (existingUser == null)
                return 0; // User not found

            // Update only the properties that should be changed
            // This preserves the ConcurrencyStamp and other EF tracking properties
            existingUser.FullName = user.FullName;
            existingUser.Gender = user.Gender;
            existingUser.DateOfJoin = user.DateOfJoin;
            existingUser.MobileNumber = user.MobileNumber;
            existingUser.Address = user.Address;
            existingUser.Email = user.Email;
            existingUser.UserName = user.UserName;
            existingUser.RoleId = user.RoleId;
            existingUser.SpecializationId = user.SpecializationId;
            existingUser.ConsultationFee = user.ConsultationFee;
            existingUser.IsActive = user.IsActive;
            existingUser.DateOfBirth = user.DateOfBirth;

            // EF will automatically track changes and update the entity
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
    }
}
