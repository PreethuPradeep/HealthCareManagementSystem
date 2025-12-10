using HealthCare.Services;
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

        public PrescriptionsController(IPrescriptionRepository repo,PdfService _pdfService)
        {
            _repo = repo;
            _pdfService = _pdfService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Search keyword is required");

            var result = await _repo.SearchByPatientOrDoctorAsync(keyword);
            return Ok(result);
        }


        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetPrescription(int prescriptionId)
        {
            var prescription = await _repo.GetPrescriptionAsync(prescriptionId);
            if (prescription == null)
                return NotFound("Prescription not found");

            return Ok(prescription);
        }


        [HttpGet("{prescriptionId}/items")]
        public async Task<IActionResult> GetPrescriptionItems(int prescriptionId)
        {
            var items = await _repo.GetPrescriptionItemsAsync(prescriptionId);
            return Ok(items);
        }

        [HttpGet("{prescriptionId}/dosage")]
        public async Task<IActionResult> GetDosageDetails(int prescriptionId)
        {
            var dosage = await _repo.GetDosageDetailsAsync(prescriptionId);
            return Ok(dosage);
        }

        [HttpGet("{prescriptionId}/download")]
        public async Task<IActionResult> DownloadPrescription(int prescriptionId)
        {
            var prescription = await _repo.GetPrescriptionAsync(prescriptionId);
            if (prescription == null) return NotFound("Prescription not found");

            var items = await _repo.GetPrescriptionItemsAsync(prescriptionId);

            // You need to inject PdfService into this controller constructor first!
            var pdfBytes = _pdfService.GeneratePrescriptionPdf(prescription, items);

            return File(pdfBytes, "application/pdf", $"Prescription_{prescriptionId}.pdf");
        }
    }
}
