using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByClinicIdAsync(int clinicId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<Appointment>> GetAppointmentsAsync(
        int? doctorId = null, 
        int? patientId = null, 
        int? clinicId = null,
        DateTime? startDate = null, 
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        int page = 1, 
        int pageSize = 20);
    Task<int> GetAppointmentCountAsync(
        int? doctorId = null, 
        int? patientId = null, 
        int? clinicId = null,
        DateTime? startDate = null, 
        DateTime? endDate = null,
        AppointmentStatus? status = null);
    Task<bool> HasConflictAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null);
    Task<IEnumerable<Appointment>> GetConflictingAppointmentsAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsForDoctorAsync(int id, int doctorId);
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate, int? doctorId = null);
}