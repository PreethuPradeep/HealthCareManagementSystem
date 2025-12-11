using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HealthCareManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IDoctorScheduleRepository _scheduleRepository;

        public DoctorSchedulesController(IDoctorScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetSchedulesByDoctor(int doctorId)
        {
            var schedules = await _scheduleRepository.GetByDoctorIdAsync(doctorId);
            return Ok(schedules);
        }

        [HttpGet("doctor/{doctorId}/available-slots")]
        [AllowAnonymous] // Allow this for appointment booking
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
        {
            try
            {
                var slots = await _scheduleRepository.GetAvailableTimeSlotsAsync(doctorId, date);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to use a logger here)
                return StatusCode(500, new { message = "An error occurred while retrieving available slots", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(DoctorSchedule schedule)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _scheduleRepository.AddAsync(schedule);
            return CreatedAtAction(nameof(GetSchedule), new { id = created.ScheduleId }, created);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, DoctorSchedule schedule)
        {
            if (id != schedule.ScheduleId)
                return BadRequest("Schedule ID mismatch.");

            var updated = await _scheduleRepository.UpdateAsync(id, schedule);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var deleted = await _scheduleRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return Ok(new { Message = "Schedule deleted" });
        }
    }
}

