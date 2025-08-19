using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public interface IPatientService
{
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientWithAppointmentsAsync(int id);
    Task<IEnumerable<Patient>> GetPatientsByClinicIdAsync(int clinicId);
    Task<(IEnumerable<Patient> Patients, int TotalCount)> SearchPatientsAsync(int clinicId, PatientSearchRequest request);
    Task<Patient?> CreatePatientAsync(int clinicId, CreatePatientRequest request);
    Task<Patient?> UpdatePatientAsync(int id, UpdatePatientRequest request);
    Task<bool> DeletePatientAsync(int id);
    Task<bool> PatientExistsInClinicAsync(int id, int clinicId);
}