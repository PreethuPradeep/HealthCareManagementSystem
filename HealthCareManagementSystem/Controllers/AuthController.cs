using HealthCare.Database;
using HealthCareManagementSystem.Helper;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Identity;
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
        private readonly HealthCare.Database.HealthCareDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenHelper jwt,
            HealthCareDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
            _db = db;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var user = await _userManager.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email == request.Email &&
                    u.IsActive);

            if (user == null || user.Role == null)
                return Unauthorized(new { message = "Invalid credentials or user role missing" });


            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password" });

            // Extract specific ID for dashboard routing
            int specificId = 0;
            string roleName = user.Role?.RoleName ?? "";

            if (roleName == "Doctor")
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                specificId = doctor?.DoctorId ?? 0;
            }
            else if (roleName == "Patient")
            {
                var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == user.Email);
                specificId = patient?.PatientId ?? 0;
            }


            var token = _jwt.GenerateToken(user, specificId);


            return Ok(new
            {
                token,
                user = new
                {
                    UserId = user.Id,
                    user.FullName,
                    user.Email,
                    Role = user.Role?.RoleName,
                    SpecificId = specificId
                }
            });

        }
    }
}
