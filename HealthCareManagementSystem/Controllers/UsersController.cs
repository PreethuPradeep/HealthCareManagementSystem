using Microsoft.AspNetCore.Mvc;
using HealthCareManagementSystem.Repository;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        // Dependency Injection
        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetActiveUsers()
        {
            var users = await _userRepository.GetAllActiveStaffAsync();

            return Ok(users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.UserName,
                u.Email,
                u.MobileNumber,
                Role = u.Role != null ? u.Role.RoleName : "No Role",
                // Include specialization and consultation fee for doctors
                Specialization = u.Specialization != null ? u.Specialization.SpecializationName : null,
                u.SpecializationId,
                u.ConsultationFee,
                u.IsActive
            }));
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateStaffDto dto)
        {
            try
            {
                await _userRepository.AddStaffAsync(dto);
                return Ok(new { Message = "User added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, ApplicationUser user)
        {
            if (id != user.Id)
                return BadRequest(new { Message = "User ID mismatch" });

            var result = await _userRepository.UpdateStaffAsync(user);

            if (result > 0)
                return Ok(new { Message = "User updated successfully" });

            return NotFound(new { Message = $"User with ID {id} not found" });
        }

        // DELETE: api/users/{id} (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userRepository.DeactivateStaffAsync(id);

            if (result > 0)
                return Ok(new { Message = $"User with ID {id} is deactivated" });

            return NotFound(new { Message = $"User with ID {id} not found" });
        }
    }
}

