using Microsoft.AspNetCore.Mvc;
using HealthCareManagementSystem.Repository;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RolesController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveRoles()
        {
            var roles = await _roleRepository.GetAllActiveRolesAsync();
            return Ok(roles.Select(r => new
            {
                r.RoleId,
                r.RoleName,
                r.IsActive
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);

            if (role == null)
                return NotFound(new { Message = $"Role with ID {id} not found" });

            return Ok(new
            {
                role.RoleId,
                role.RoleName,
                role.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Role created)
        {
            created.IsActive = true;

            await _roleRepository.AddRoleAsync(created);
            return Ok(new { Message = "Role added successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            if (id != role.RoleId)
                return BadRequest(new { Message = "Role ID mismatch" });

            var result = await _roleRepository.UpdateRoleAsync(role);

            if (result > 0)
                return Ok(new { Message = "Role updated successfully" });

            return NotFound(new { Message = $"Role with ID {id} not found" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _roleRepository.DeactivateRoleAsync(id);

            if (result > 0)
                return Ok(new { Message = $"Role with ID {id} deactivated" });

            return NotFound(new { Message = $"Role with ID {id} not found" });
        }
    }
}
