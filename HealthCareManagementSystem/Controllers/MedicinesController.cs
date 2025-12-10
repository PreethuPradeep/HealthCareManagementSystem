using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Pharmacist")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineRepository _repo;

        public MedicinesController(IMedicineRepository repo)
        {
            _repo = repo;
        }

        // 1. Get ALL medicines – used only for admin views
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var medicines = await _repo.GetAllAsync();
            return Ok(medicines);
        }

        // 2. Lightweight list for dropdowns / prescriptions
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var list = await _repo.GetListAsync();
            return Ok(list);
        }

        // 3. Get full medicine details (DTO)
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var details = await _repo.GetDetailsAsync(id);
            if (details == null)
                return NotFound("Medicine not found");

            return Ok(details);
        }

        // 4. Search medicines
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var results = await _repo.SearchAsync(query);
            return Ok(results);
        }

        // 5. Get Medicine by ID (full entity)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var medicine = await _repo.GetByIdAsync(id);
            if (medicine == null)
                return NotFound("Medicine not found");

            return Ok(medicine);
        }

        // 6. Add Medicine
        [HttpPost]
        public async Task<IActionResult> Add(Medicine medicine)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _repo.AddAsync(medicine);
            return Ok(result);
        }

        // 7. Update Medicine
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Medicine medicine)
        {
            if (id != medicine.MedicineId)
                return BadRequest("Medicine ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _repo.UpdateAsync(medicine);

            if (updated == null)
                return NotFound("Medicine not found");

            return Ok(updated);
        }

        // 8. Delete Medicine
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repo.DeleteAsync(id);
            if (!success)
                return NotFound("Medicine not found");

            return Ok(new { Message = "Medicine deleted successfully" });
        }

        // 9. Check Stock
        [HttpGet("check-stock/{medicineId}/{quantity}")]
        public async Task<IActionResult> CheckStock(int medicineId, int quantity)
        {
            var status = await _repo.CheckStockAsync(medicineId, quantity);
            return Ok(status);
        }
    }
}
