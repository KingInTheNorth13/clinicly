using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByIdWithAppointmentsAsync(int id);
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Patient>> SearchPatientsAsync(int clinicId, string? query = null, string? name = null, string? phone = null, string? email = null, int page = 1, int pageSize = 20);
    Task<int> GetPatientCountAsync(int clinicId, string? query = null, string? name = null, string? phone = null, string? email = null);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsInClinicAsync(int id, int clinicId);
}