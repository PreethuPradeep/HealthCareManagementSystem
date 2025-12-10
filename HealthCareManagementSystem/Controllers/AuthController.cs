using HealthCareManagementSystem.Helper;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenHelper _jwt;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenHelper jwt)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            // Include related Role so token has role claim without null reference issues.
            var user = await _userManager.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password" });
            int specificId = 0;
            string roleName = user.Role?.RoleName ?? "";

            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HealthCare.Database.HealthCareDbContext>();

                if (roleName == "Doctor")
                {
                    var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                    specificId = doctor?.DoctorId ?? 0;
                }
                else if (roleName == "Patient")
                {
                    var patient = await db.Patients.FirstOrDefaultAsync(p => p.Email == user.Email);
                    specificId = patient?.PatientId ?? 0;
                }
                // Add logic for Pharmacist/Receptionist if they have specific tables
            }
            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Role = user.Role?.RoleName
                }
            });
        }
    }
}

