using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public enum AppointmentStatus
{
    Scheduled,
    Completed,
    Cancelled,
    NoShow
}

public class Appointment
{
    public int Id { get; set; }
    
    public int DoctorId { get; set; }
    
    public int PatientId { get; set; }
    
    [Required]
    public DateTime DateTime { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? ReminderJobId { get; set; }
    
    // Navigation properties
    public Doctor Doctor { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
}