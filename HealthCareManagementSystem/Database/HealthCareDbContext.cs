using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Models.Pharm;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Database
{
    public class HealthCareDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public HealthCareDbContext(DbContextOptions<HealthCareDbContext> options)
            : base(options)
        {
        }

        //  GROUP COMMON MODELS
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        //  PHARMACIST MODULE MODELS
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }

        public DbSet<PharmacyBill> PharmacyBills { get; set; }
        public DbSet<PharmacyBillItem> PharmacyBillItems { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        //Receptionist
        public DbSet<Billing> Billings { get; set; }
        public DbSet<LabTestRequest> LabTestRequests { get; set; }
        //admin
        public DbSet<Role> Roles { get; set; }
        public DbSet<Specialization> Specializations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RELATIONSHIPS THAT DO NOT REQUIRE MODIFYING GROUP MODELS
            //admin config relationships
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany(r => r.ApplicationUsers)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Specialization)
                .WithMany(s => s.ApplicationUsers)
                .HasForeignKey(u => u.SpecializationId)
                .OnDelete(DeleteBehavior.SetNull);

            // PharmacyBill → BillItems (1 → Many)
            modelBuilder.Entity<PharmacyBill>()
                .HasMany(b => b.Items)
                .WithOne(i => i.PharmacyBill)
                .HasForeignKey(i => i.PharmacyBillId);

            // Medicine → BillItems (1 → Many)
            modelBuilder.Entity<Medicine>()
                .HasMany(m => m.BillItems)
                .WithOne(bi => bi.Medicine)
                .HasForeignKey(bi => bi.MedicineId);

            // Medicine → StockTransactions (1 → Many)
            modelBuilder.Entity<Medicine>()
                .HasMany(m => m.StockTransactions)
                .WithOne(st => st.Medicine)
                .HasForeignKey(st => st.MedicineId);
            // Configure Consultation foreign keys to avoid cascade path conflicts
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Patient)
                .WithMany()
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Doctor)
                .WithMany(d => d.Consultations)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Appointment)
                .WithMany()
                .HasForeignKey(c => c.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            //doctor consultation
            modelBuilder.Entity<Consultation>()
                .HasMany(c => c.Prescriptions)
                .WithOne(p => p.Consultation)
                .HasForeignKey(p => p.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Consultation>()
                .HasMany(c => c.LabTests)
                .WithOne(l => l.Consultation)
                .HasForeignKey(l => l.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision for Doctor.Fee
            modelBuilder.Entity<Doctor>()
                .Property(d => d.Fee)
                .HasPrecision(18, 2);

            // DoctorSchedule relationship
            modelBuilder.Entity<DoctorSchedule>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
