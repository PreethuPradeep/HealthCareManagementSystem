using HealthCare.Models.DTOs;
using HealthCare.Services;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionRepository _repo;
        private readonly PdfService _pdfService;

        public PrescriptionsController(IPrescriptionRepository repo, PdfService pdfService)
        {
            _repo = repo;
            _pdfService = pdfService;
        }

        // 1. Search prescriptions (by patient or doctor)
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Search keyword is required");

            var result = await _repo.SearchByPatientOrDoctorAsync(keyword);
            return Ok(result);
        }

        // 2. Get full prescription details (DTO)
        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetPrescription(int prescriptionId)
        {
            var details = await _repo.GetPrescriptionDetailsAsync(prescriptionId);

            if (details == null)
                return NotFound("Prescription not found");

            return Ok(details);
        }

        // 3. Download PDF
        [HttpGet("{prescriptionId}/download")]
        public async Task<IActionResult> DownloadPrescription(int prescriptionId)
        {
            var details = await _repo.GetPrescriptionDetailsAsync(prescriptionId);

            if (details == null)
                return NotFound("Prescription not found");

            // Convert DTO → Entity-like structure for PDF
            var prescriptionEntity = new Prescription
            {
                PrescriptionId = details.PrescriptionId,
                ConsultationId = details.ConsultationId
            };

            var items = details.Medicines.Select(m => new PrescriptionItem
            {
                MedicineId = m.MedicineId,
                MorningDose = m.MorningDose,
                NoonDose = m.NoonDose,
                EveningDose = m.EveningDose,
                MealTime = m.MealTime,
                DurationInDays = m.DurationInDays,
                Dosage = m.Dosage,
                Quantity = m.Quantity,

                Medicine = new Medicine
                {
                    Name = m.MedicineName
                }
            }).ToList();

            var pdfBytes = _pdfService.GeneratePrescriptionPdf(
                prescriptionEntity,
                items
            );

            return File(pdfBytes, "application/pdf", $"Prescription_{prescriptionId}.pdf");
        }
    }
}
