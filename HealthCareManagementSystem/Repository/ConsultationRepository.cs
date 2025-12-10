using HealthCare.Database;
using HealthCare.Models.DTOs;
using HealthCareManagementSystem.Models;
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

        public async Task<Consultation> AddConsultationAsync(ConsultationRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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

                if (request.Medicines.Any())
                {
                    var prescription = new Prescription
                    {
                        ConsultationId = consultation.ConsultationId,
                        MedicineName = "General"
                    };
                    _context.Prescriptions.Add(prescription);
                    await _context.SaveChangesAsync();

                    foreach (var med in request.Medicines)
                    {
                        var item = new PrescriptionItem
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
                        };
                        _context.PrescriptionItems.Add(item);
                    }
                }

                if (request.LabTests.Any())
                {
                    foreach (var test in request.LabTests)
                    {
                        var labRequest = new LabTestRequest
                        {
                            PatientId = request.PatientId,
                            DoctorId = request.DoctorId,
                            TestName = test.TestName,
                            RequestedAt = DateTime.UtcNow
                        };
                        _context.LabTestRequests.Add(labRequest);
                    }
                }

                var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
                if (appointment != null)
                {
                    appointment.IsVisited = true;
                    _context.Appointments.Update(appointment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return consultation;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Consultation?> GetByIdAsync(int id)
        {
            return await _context.Consultations.FindAsync(id);
        }

        public async Task<bool> UpdateConsultationAsync(int id, ConsultationRequestDTO request)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return false;

            // 1. Update Basic Fields
            consultation.Symptoms = request.Symptoms;
            consultation.Diagnosis = request.Diagnosis;
            consultation.DoctorNotes = request.DoctorNotes;
            consultation.FollowUpDate = request.FollowUpDate;

            // 2. Handle Medicines (Clear old, Add new)
            // Find the prescription linked to this consultation
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionItems)
                .FirstOrDefaultAsync(p => p.ConsultationId == id);

            if (prescription != null)
            {
                // Remove old items
                _context.PrescriptionItems.RemoveRange(prescription.PrescriptionItems);

                // Add new items from request
                foreach (var med in request.Medicines)
                {
                    var item = new PrescriptionItem
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
                    };
                    _context.PrescriptionItems.Add(item);
                }
            }

            // 3. Handle Lab Tests (Clear old, Add new)
            // Be careful not to delete tests that might have results already (if you have results logic). 
            // For now, we assume simple replace logic.
            var oldLabs = await _context.LabTestRequests
                .Where(l => l.PatientId == request.PatientId && l.DoctorId == request.DoctorId && l.RequestedAt.Date == consultation.DateBooked.Date)
                .ToListAsync();

            _context.LabTestRequests.RemoveRange(oldLabs);

            foreach (var test in request.LabTests)
            {
                var labRequest = new LabTestRequest
                {
                    PatientId = request.PatientId,
                    DoctorId = request.DoctorId,
                    TestName = test.TestName,
                    RequestedAt = DateTime.UtcNow
                };
                _context.LabTestRequests.Add(labRequest);
            }

            _context.Consultations.Update(consultation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteConsultationAsync(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null) return false;

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