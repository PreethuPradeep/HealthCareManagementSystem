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
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserSqlServerRepositoryImpl(
            HealthCareDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
            // Ensure a password exists (UI currently does not send one) and it meets basic policy (digit + length)
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                model.Password = "Welcome123"; // temporary bootstrap password; should be reset on first login
            }
            // If password lacks a digit, append one to satisfy policy
            if (!model.Password.Any(char.IsDigit))
            {
                model.Password += "1";
            }
            // Ensure minimum length of 6
            if (model.Password.Length < 6)
            {
                model.Password = model.Password.PadRight(6, '1');
            }

            var identityUser = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                NormalizedUserName = model.UserName?.ToUpperInvariant(),
                NormalizedEmail = model.Email?.ToUpperInvariant(),
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

            // Add user to Identity role if RoleId is provided
            if (model.RoleId.HasValue)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == model.RoleId.Value);
                if (role != null && !string.IsNullOrWhiteSpace(role.RoleName))
                {
                    // Ensure Identity role exists
                    var identityRoleExists = await _roleManager.RoleExistsAsync(role.RoleName);
                    if (!identityRoleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role.RoleName));
                    }
                    
                    // Add user to Identity role
                    var addToRoleResult = await _userManager.AddToRoleAsync(identityUser, role.RoleName);
                    if (!addToRoleResult.Succeeded)
                    {
                        // Log warning but don't fail user creation
                        var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"Warning: Failed to add user to Identity role '{role.RoleName}': {roleErrors}");
                    }
                }
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
            existingUser.NormalizedEmail = user.Email?.ToUpperInvariant();
            existingUser.NormalizedUserName = user.UserName?.ToUpperInvariant();
            var oldRoleId = existingUser.RoleId;
            existingUser.RoleId = user.RoleId;
            existingUser.SpecializationId = user.SpecializationId;
            existingUser.ConsultationFee = user.ConsultationFee;
            existingUser.IsActive = user.IsActive;
            existingUser.DateOfBirth = user.DateOfBirth;

            // Sync Identity roles if RoleId changed
            if (oldRoleId != user.RoleId)
            {
                // Remove from old Identity role
                if (oldRoleId.HasValue)
                {
                    var oldRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == oldRoleId.Value);
                    if (oldRole != null && !string.IsNullOrWhiteSpace(oldRole.RoleName))
                    {
                        var isInOldRole = await _userManager.IsInRoleAsync(existingUser, oldRole.RoleName);
                        if (isInOldRole)
                        {
                            await _userManager.RemoveFromRoleAsync(existingUser, oldRole.RoleName);
                        }
                    }
                }

                // Add to new Identity role
                if (user.RoleId.HasValue)
                {
                    var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == user.RoleId.Value);
                    if (newRole != null && !string.IsNullOrWhiteSpace(newRole.RoleName))
                    {
                        // Ensure Identity role exists
                        var identityRoleExists = await _roleManager.RoleExistsAsync(newRole.RoleName);
                        if (!identityRoleExists)
                        {
                            await _roleManager.CreateAsync(new IdentityRole(newRole.RoleName));
                        }
                        
                        // Add user to Identity role
                        var isInNewRole = await _userManager.IsInRoleAsync(existingUser, newRole.RoleName);
                        if (!isInNewRole)
                        {
                            var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, newRole.RoleName);
                            if (!addToRoleResult.Succeeded)
                            {
                                var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                                Console.WriteLine($"Warning: Failed to add user to Identity role '{newRole.RoleName}': {roleErrors}");
                            }
                        }
                    }
                }
            }

            // EF will automatically track changes and update the entity
            return await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Specialization)
                .FirstOrDefaultAsync(u => u.Id == id);
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
