using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(150)]
        [RegularExpression(@"^[A-Za-z]+(?: [A-Za-z]+)*$",
            ErrorMessage = "Name must contain only letters and a single space between words.")]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfJoin { get; set; }

        [Required]
        [StringLength(15)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter 10 digit mobile number")]
        public string MobileNumber { get; set; }

        public string? Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Clinic-specific role (separate from Identity roles)
        public int? RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        // Optional: Only for Doctors
        public int? SpecializationId { get; set; }

        [ForeignKey(nameof(SpecializationId))]
        public Specialization? Specialization { get; set; }

        // Consultation fee for doctors
        [Column(TypeName = "decimal(10,2)")]
        public decimal? ConsultationFee { get; set; }

        public bool IsActive { get; set; } = true;
      
    }
}

