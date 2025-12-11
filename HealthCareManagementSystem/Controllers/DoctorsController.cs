using HealthCare.Database;
using HealthCareManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly HealthCareDbContext _context;

        public DoctorsController(HealthCareDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            // Get users where role is "Doctor"
            var doctorUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Specialization)
                .Where(u => u.IsActive 
                    && u.Role != null 
                    && u.Role.RoleName == "Doctor")
                .ToListAsync();

            if (!doctorUsers.Any())
            {
                return Ok(new List<object>());
            }

            // Load all existing Doctor records for these users in one query
            var userIds = doctorUsers.Select(u => u.Id).ToList();
            var existingDoctors = await _context.Doctors
                .Where(d => userIds.Contains(d.UserId))
                .ToDictionaryAsync(d => d.UserId);

            // Create missing Doctor records
            var doctorsToCreate = new List<Doctor>();
            foreach (var user in doctorUsers)
            {
                if (!existingDoctors.ContainsKey(user.Id))
                {
                    // Ensure user has a specialization (required for Doctor table)
                    if (user.SpecializationId == null)
                    {
                        // Skip users without specialization - they need to be configured first
                        continue;
                    }

                    var newDoctor = new Doctor
                    {
                        UserId = user.Id,
                        SpecializationId = user.SpecializationId.Value,
                        Fee = user.ConsultationFee ?? 0,
                        IsActive = true
                    };

                    doctorsToCreate.Add(newDoctor);
                    existingDoctors[user.Id] = newDoctor;
                }
            }

            // Batch create missing Doctor records
            if (doctorsToCreate.Any())
            {
                _context.Doctors.AddRange(doctorsToCreate);
                await _context.SaveChangesAsync();
            }

            // Build results
            var doctorResults = doctorUsers
                .Where(u => existingDoctors.ContainsKey(u.Id) && existingDoctors[u.Id].IsActive)
                .Select(user => new
                {
                    DoctorId = existingDoctors[user.Id].DoctorId,
                    FullName = user.FullName,
                    Specialization = user.Specialization != null ? user.Specialization.SpecializationName : null,
                    Fee = existingDoctors[user.Id].Fee,
                    IsActive = existingDoctors[user.Id].IsActive
                })
                .ToList();

            return Ok(doctorResults);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                    .ThenInclude(u => u!.Role)
                .Include(d => d.Specialization)
                .Where(d => d.DoctorId == id 
                    && d.IsActive 
                    && d.User != null 
                    && d.User.IsActive 
                    && d.User.Role != null 
                    && d.User.Role.RoleName == "Doctor")
                .Select(d => new
                {
                    d.DoctorId,
                    FullName = d.User != null ? d.User.FullName : "Unknown",
                    Specialization = d.Specialization != null ? d.Specialization.SpecializationName : null,
                    d.Fee,
                    d.IsActive
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }
    }
}

