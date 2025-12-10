namespace HealthCare.Models.DTOs
{
    public class PrescriptionDetailsDTO
    {
        public int PrescriptionId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime DateIssued { get; set; }

        public List<PrescriptionMedicineDTO> Medicines { get; set; } = new();
        public int ConsultationId { get; internal set; }
    }

    public class PrescriptionMedicineDTO
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;

        public string MealTime { get; set; } = string.Empty;
        public int MorningDose { get; set; }
        public int NoonDose { get; set; }
        public int EveningDose { get; set; }

        public int? Quantity { get; set; }
        public int? Dosage { get; set; }
        public int DurationInDays { get; set; }
    }
}
