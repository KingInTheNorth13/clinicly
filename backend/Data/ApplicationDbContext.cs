using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Clinic entity
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Doctor entity
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Specialization).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Unique constraint on email
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Foreign key relationship with Clinic
            entity.HasOne(d => d.Clinic)
                  .WithMany(c => c.Doctors)
                  .HasForeignKey(d => d.ClinicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Patient entity
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Foreign key relationship with Clinic
            entity.HasOne(p => p.Clinic)
                  .WithMany(c => c.Patients)
                  .HasForeignKey(p => p.ClinicId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Unique constraint on email
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Convert enum to string
            entity.Property(e => e.Role)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            
            // Foreign key relationship with Clinic
            entity.HasOne(u => u.Clinic)
                  .WithMany(c => c.Users)
                  .HasForeignKey(u => u.ClinicId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Optional foreign key relationship with Doctor
            entity.HasOne(u => u.Doctor)
                  .WithOne(d => d.User)
                  .HasForeignKey<User>(u => u.DoctorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Appointment entity
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DateTime).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Convert enum to string
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(50)
                  .HasDefaultValue(AppointmentStatus.Scheduled);
            
            // Unique constraint for doctor_id + datetime to prevent double booking
            entity.HasIndex(e => new { e.DoctorId, e.DateTime })
                  .IsUnique()
                  .HasDatabaseName("IX_Appointments_Doctor_DateTime");
            
            // Foreign key relationship with Doctor
            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key relationship with Patient
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Create indexes for performance optimization
        modelBuilder.Entity<Appointment>()
            .HasIndex(e => e.PatientId)
            .HasDatabaseName("IX_Appointments_Patient");
        
        modelBuilder.Entity<Patient>()
            .HasIndex(e => e.ClinicId)
            .HasDatabaseName("IX_Patients_Clinic");
        
        modelBuilder.Entity<Doctor>()
            .HasIndex(e => e.ClinicId)
            .HasDatabaseName("IX_Doctors_Clinic");
    }
}