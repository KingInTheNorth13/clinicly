using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.DTOs;

public class CreatePatientRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? Notes { get; set; }
}

public class UpdatePatientRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? Notes { get; set; }
}

public class PatientResponse
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AppointmentCount { get; set; }
}

public class PatientSearchRequest
{
    public string? Query { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PatientWithAppointmentsResponse : PatientResponse
{
    public IEnumerable<AppointmentSummary> Appointments { get; set; } = new List<AppointmentSummary>();
}

public class AppointmentSummary
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}