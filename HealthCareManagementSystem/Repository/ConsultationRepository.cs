using HealthCare.Database;
using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
using Microsoft.EntityFrameworkCore;

namespace HealthCareManagementSystem.Repository
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly HealthCareDbContext _context;

        public ConsultationRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<Consultation?> GetByIdAsync(int id)
        {
            return await _context.Consultations.FindAsync(id);
        }

        public async Task<Consultation?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.User)
                .Include(c => c.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                        .ThenInclude(i => i.Medicine)
                .Include(c => c.LabTests)
                .FirstOrDefaultAsync(c => c.ConsultationId == id);
        }

        public async Task<Consultation> AddConsultationAsync(ConsultationRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var consultation = new Consultation
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentId = request.AppointmentId,
                ChiefComplaint = request.ChiefComplaint,
                Symptoms = request.Symptoms,
                Diagnosis = request.Diagnosis,
                DoctorNotes = request.DoctorNotes,
                FollowUpDate = request.FollowUpDate,
                DateBooked = DateTime.UtcNow
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            // Create Prescription
            if (request.Medicines.Any())
            {
                var prescription = new Prescription
                {
                    ConsultationId = consultation.ConsultationId,
                    MedicineName = "Prescription"
                };
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();

                foreach (var med in request.Medicines)
                {
                    _context.PrescriptionItems.Add(new PrescriptionItem
                    {
                        PrescriptionId = prescription.PrescriptionId,
                        MedicineId = med.MedicineId,
                        Quantity = med.Quantity,
                        Dosage = med.Dosage,
                        DurationInDays = med.DurationInDays,
                        MorningDose = med.MorningDose,
                        NoonDose = med.NoonDose,
                        EveningDose = med.EveningDose,
                        MealTime = med.MealTime
                    });
                }
            }

            // Create Lab Tests
            foreach (var test in request.LabTests)
            {
                _context.LabTestRequests.Add(new LabTestRequest
                {
                    ConsultationId = consultation.ConsultationId,
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    TestName = test.TestName,
                    RequestedAt = DateTime.UtcNow
                });
            }

            // Mark appointment visited
            var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
            if (appointment != null)
            {
                appointment.IsVisited = true;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return consultation;
        }

        public async Task<IEnumerable<PatientHistoryDTO>> GetPatientHistoryAsync(int patientId)
        {
            return await _context.Consultations
                .Where(c => c.PatientId == patientId)
                .Include(c => c.Doctor)
                    .ThenInclude(d => d.User)
                .OrderByDescending(c => c.DateBooked)
                .Select(c => new PatientHistoryDTO
                {
                    ConsultationId = c.ConsultationId,
                    VisitDate = c.DateBooked,
                    Diagnosis = c.Diagnosis,
                    ChiefComplaint = c.ChiefComplaint,
                    DoctorName = c.Doctor.User.FullName
                })
                .ToListAsync();
        }
        public async Task<bool> UpdateConsultationAsync(int id, ConsultationRequestDTO request)
        {
            var consultation = await _context.Consultations
                .Include(c => c.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                .Include(c => c.LabTests)
                .FirstOrDefaultAsync(c => c.ConsultationId == id);

            if (consultation == null)
                return false;

            // Update basic SOAP fields
            consultation.ChiefComplaint = request.ChiefComplaint;
            consultation.Symptoms = request.Symptoms;
            consultation.Diagnosis = request.Diagnosis;
            consultation.DoctorNotes = request.DoctorNotes;
            consultation.FollowUpDate = request.FollowUpDate;

            // -------------------------
            // 1) Update Prescription
            // -------------------------

            var prescription = consultation.Prescriptions?.FirstOrDefault();

            if (prescription == null && request.Medicines.Any())
            {
                // Create new if missing
                prescription = new Prescription
                {
                    ConsultationId = consultation.ConsultationId,
                    MedicineName = "Prescription"
                };
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
            }

            if (prescription != null)
            {
                // Remove old items
                _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);

                // Add new items
                foreach (var med in request.Medicines)
                {
                    _context.PrescriptionItems.Add(new PrescriptionItem
                    {
                        PrescriptionId = prescription.PrescriptionId,
                        MedicineId = med.MedicineId,
                        Quantity = med.Quantity,
                        Dosage = med.Dosage,
                        DurationInDays = med.DurationInDays,
                        MorningDose = med.MorningDose,
                        NoonDose = med.NoonDose,
                        EveningDose = med.EveningDose,
                        MealTime = med.MealTime
                    });
                }
            }

            // -------------------------
            // 2) Update Lab Tests
            // -------------------------

            // Remove existing linked lab tests
            if (consultation.LabTests != null && consultation.LabTests.Any())
                _context.LabTestRequests.RemoveRange(consultation.LabTests);

            // Add the new lab tests
            foreach (var test in request.LabTests)
            {
                _context.LabTestRequests.Add(new LabTestRequest
                {
                    ConsultationId = consultation.ConsultationId,
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    TestName = test.TestName,
                    RequestedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteConsultationAsync(int id)
        {
            var consultation = await _context.Consultations
                .Include(c => c.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                .Include(c => c.LabTests)
                .FirstOrDefaultAsync(c => c.ConsultationId == id);

            if (consultation == null)
                return false;

            // Remove prescription items first
            if (consultation.Prescriptions != null)
            {
                foreach (var pres in consultation.Prescriptions)
                {
                    if (pres.PrescriptionItems.Any())
                        _context.PrescriptionItems.RemoveRange(pres.PrescriptionItems);
                }

                _context.Prescriptions.RemoveRange(consultation.Prescriptions);
            }

            // Remove lab tests
            if (consultation.LabTests != null && consultation.LabTests.Any())
                _context.LabTestRequests.RemoveRange(consultation.LabTests);

            // Reset appointment visit status
            var appointment = await _context.Appointments.FindAsync(consultation.AppointmentId);
            if (appointment != null)
            {
                appointment.IsVisited = false;
            }

            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
