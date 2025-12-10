using HealthCare.Database;
using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly HealthCareDbContext _context;

        public PrescriptionRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PrescriptionListDTO>> SearchByPatientOrDoctorAsync(string keyword)
        {
            keyword = keyword.ToLower();

            return await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Doctor)
                        .ThenInclude(d => d.User)
                .Where(p =>
                    p.Consultation.Patient.FullName.ToLower().Contains(keyword) ||
                    p.Consultation.Doctor.User.FullName.ToLower().Contains(keyword))
                .Select(p => new PrescriptionListDTO
                {
                    PrescriptionId = p.PrescriptionId,
                    PatientName = p.Consultation.Patient.FullName,
                    DoctorName = p.Consultation.Doctor.User.FullName,
                    DateIssued = p.Consultation.DateBooked
                })
                .ToListAsync();
        }

        public async Task<PrescriptionDetailsDTO?> GetPrescriptionDetailsAsync(int prescriptionId)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.PrescriptionItems)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

            if (prescription == null) return null;

            return new PrescriptionDetailsDTO
            {
                PrescriptionId = prescription.PrescriptionId,
                PatientName = prescription.Consultation.Patient.FullName,
                DoctorName = prescription.Consultation.Doctor.User.FullName,
                DateIssued = prescription.Consultation.DateBooked,
                Medicines = prescription.PrescriptionItems.Select(i => new PrescriptionMedicineDTO
                {
                    MedicineId = i.MedicineId,
                    MedicineName = i.Medicine.Name,
                    MealTime = i.MealTime,
                    MorningDose = i.MorningDose,
                    NoonDose = i.NoonDose,
                    EveningDose = i.EveningDose,
                    DurationInDays = i.DurationInDays,
                    Quantity = i.Quantity,
                    Dosage = i.Dosage
                }).ToList()
            };
        }
    }
}
