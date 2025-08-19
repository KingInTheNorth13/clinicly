using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class Clinic
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Address { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<User> Users { get; set; } = new List<User>();
}