namespace HealthCareManagementSystem.Models
{
    public class CreateStaffDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfJoin { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string Address { get; set; }
        public int RoleId { get; set; }
        public int? SpecializationId { get; set; }
        public decimal? ConsultationFee { get; set; }
        public bool IsActive { get; set; }

        public string Password { get; set; }
    }
}

