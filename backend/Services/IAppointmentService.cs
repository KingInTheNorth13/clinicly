using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public interface IAppointmentService
{
    Task<Appointment?> GetAppointmentByIdAsync(int id);
    Task<AppointmentDetailsResponse?> GetAppointmentDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByClinicIdAsync(int clinicId, DateTime? startDate = null, DateTime? endDate = null);
    Task<(IEnumerable<AppointmentResponse> Appointments, int TotalCount)> SearchAppointmentsAsync(AppointmentSearchRequest request, int? restrictToDoctorId = null);
    Task<AppointmentConflictResponse> CheckAppointmentConflictAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null);
    Task<Appointment?> CreateAppointmentAsync(int doctorId, CreateAppointmentRequest request);
    Task<Appointment?> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request, int? restrictToDoctorId = null);
    Task<bool> DeleteAppointmentAsync(int id, int? restrictToDoctorId = null);
    Task<bool> AppointmentExistsForDoctorAsync(int id, int doctorId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate, int? doctorId = null);
    Task<bool> ValidateAppointmentAccessAsync(int appointmentId, int userId, UserRole userRole);
}