using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        [HttpGet("welcome")]
        public IActionResult Welcome()
        {
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "User";
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Unknown";
            var specificId = User.Claims.FirstOrDefault(c => c.Type == "SpecificId")?.Value ?? "0";

            return Ok(new
            {
                message = $"Welcome, {name}! You are signed in as {role}.",
                specificId
            });
        }
    }
}
