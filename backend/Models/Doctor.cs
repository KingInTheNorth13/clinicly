using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class Doctor
{
    public int Id { get; set; }
    
    public int ClinicId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Specialization { get; set; }
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Clinic Clinic { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public User? User { get; set; }
}