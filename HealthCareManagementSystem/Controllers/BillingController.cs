using HealthCare.Database;
using HealthCare.Models.DTOs;
using HealthCare.Services;
using HealthCareManagementSystem.Models.Pharm;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PharmacyBillingController : ControllerBase
    {
        private readonly IBillingPharmacyRepository _billingRepo;
        private readonly IMedicineRepository _medicineRepo;
        private readonly PdfService _pdfService;

        public PharmacyBillingController(
            IBillingPharmacyRepository billingRepo,
            IMedicineRepository medicineRepo,
            PdfService pdfService)
        {
            _billingRepo = billingRepo;
            _medicineRepo = medicineRepo;
            _pdfService = pdfService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBill([FromBody] CreatePharmacyBillDTO dto)
        {
            if (dto.Items.Count == 0)
                return BadRequest("Bill must contain at least one item.");

            var billItems = new List<PharmacyBillItem>();

            foreach (var i in dto.Items)
            {
                var med = await _medicineRepo.GetByIdAsync(i.MedicineId);
                if (med == null)
                    return BadRequest($"Invalid MedicineId: {i.MedicineId}");

                // Check stock
                var hasStock = await _medicineRepo.CheckStockAsync(i.MedicineId, i.Quantity);
                if (!hasStock)
                    return BadRequest($"Insufficient stock for MedicineId {i.MedicineId}");

                var item = new PharmacyBillItem
                {
                    MedicineId = i.MedicineId,
                    Quantity = i.Quantity,
                    UnitPrice = med.SellingPrice,
                    LineTotal = med.SellingPrice * i.Quantity
                };

                billItems.Add(item);
            }

            var bill = new PharmacyBill();
            var saved = await _billingRepo.CreateBillAsync(bill, billItems);

            return Ok(saved);
        }

        [HttpGet("download/{billId}")]
        public async Task<IActionResult> DownloadBill(int billId)
        {
            var bill = await _billingRepo.GetBillByIdAsync(billId);
            if (bill == null)
                return NotFound("Bill not found.");

            var pdfBytes = _pdfService.GenerateBillPdf(bill);

            return File(pdfBytes, "application/pdf", $"PharmacyBill_{billId}.pdf");
        }
    }
}
