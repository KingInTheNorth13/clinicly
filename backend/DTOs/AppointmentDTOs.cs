using System.ComponentModel.DataAnnotations;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.DTOs;

public class CreateAppointmentRequest
{
    [Required]
    public int PatientId { get; set; }
    
    [Required]
    public DateTime DateTime { get; set; }
    
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    public int? PatientId { get; set; }
    
    public DateTime? DateTime { get; set; }
    
    public AppointmentStatus? Status { get; set; }
    
    public string? Notes { get; set; }
}

public class AppointmentSearchRequest
{
    public int? DoctorId { get; set; }
    
    public int? PatientId { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public AppointmentStatus? Status { get; set; }
    
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 20;
}

public class AppointmentResponse
{
    public int Id { get; set; }
    
    public int DoctorId { get; set; }
    
    public string DoctorName { get; set; } = string.Empty;
    
    public int PatientId { get; set; }
    
    public string PatientName { get; set; } = string.Empty;
    
    public string? PatientPhone { get; set; }
    
    public string? PatientEmail { get; set; }
    
    public DateTime DateTime { get; set; }
    
    public AppointmentStatus Status { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class AppointmentDetailsResponse : AppointmentResponse
{
    public string? DoctorSpecialization { get; set; }
    
    public string ClinicName { get; set; } = string.Empty;
    
    public string? ClinicAddress { get; set; }
}

public class AppointmentConflictResponse
{
    public bool HasConflict { get; set; }
    
    public IEnumerable<AppointmentResponse> ConflictingAppointments { get; set; } = new List<AppointmentResponse>();
    
    public IEnumerable<DateTime> SuggestedTimes { get; set; } = new List<DateTime>();
}