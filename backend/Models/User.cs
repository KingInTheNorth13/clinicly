using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public enum UserRole
{
    Doctor,
    Admin
}

public class User
{
    public int Id { get; set; }
    
    public int ClinicId { get; set; }
    
    public int? DoctorId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public Doctor? Doctor { get; set; }
}