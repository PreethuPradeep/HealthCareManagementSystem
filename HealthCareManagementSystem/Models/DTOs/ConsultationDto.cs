namespace HealthCare.Models.DTOs
{
    public class ConsultationRequestDTO
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }

        // SOAP Details
        public string? ChiefComplaint { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorNotes { get; set; }
        public DateTime? FollowUpDate { get; set; }

        // Lists for Prescriptions and Lab Tests
        public List<PrescriptionItemDTO> Medicines { get; set; } = new();
        public List<LabTestDTO> LabTests { get; set; } = new();
    }

    public class PrescriptionItemDTO
    {
        public int MedicineId { get; set; }
        public int? Quantity { get; set; }
        public int? Dosage { get; set; } // e.g., 1-0-1 could be mapped or just ID
        public string? Remarks { get; set; }
        public int DurationInDays { get; set; }

        public int MorningDose { get; set; }
        public int NoonDose { get; set; }
        public int EveningDose { get; set; }
        public string MealTime { get; set; } = "After Food";
    }

    public class LabTestDTO
    {
        public string TestName { get; set; } = string.Empty;
    }

    // DTO for Patient History Cards
    public class PatientHistoryDTO
    {
        public int ConsultationId { get; set; }
        public DateTime VisitDate { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorName { get; set; }
        public string? ChiefComplaint { get; set; }
    }

    public class PatientHistoryDetailDTO : PatientHistoryDTO
    {
        public string? Symptoms { get; set; }
        public string? DoctorNotes { get; set; }
        public List<PatientHistoryPrescriptionDTO> Medicines { get; set; } = new();
        public List<string> LabTests { get; set; } = new();
    }

    public class PatientHistoryPrescriptionDTO
    {
        public int MedicineId { get; set; }
        public string? MedicineName { get; set; }
        public int? Quantity { get; set; }
        public int DurationInDays { get; set; }
        public int MorningDose { get; set; }
        public int NoonDose { get; set; }
        public int EveningDose { get; set; }
        public string MealTime { get; set; } = "After Food";
    }
}
