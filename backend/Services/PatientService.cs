using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;

namespace ClinicAppointmentSystem.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        IPatientRepository patientRepository,
        ILogger<PatientService> logger)
    {
        _patientRepository = patientRepository;
        _logger = logger;
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<Patient?> GetPatientWithAppointmentsAsync(int id)
    {
        return await _patientRepository.GetByIdWithAppointmentsAsync(id);
    }

    public async Task<IEnumerable<Patient>> GetPatientsByClinicIdAsync(int clinicId)
    {
        return await _patientRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<(IEnumerable<Patient> Patients, int TotalCount)> SearchPatientsAsync(int clinicId, PatientSearchRequest request)
    {
        try
        {
            var patients = await _patientRepository.SearchPatientsAsync(
                clinicId,
                request.Query,
                request.Name,
                request.Phone,
                request.Email,
                request.Page,
                request.PageSize);

            var totalCount = await _patientRepository.GetPatientCountAsync(
                clinicId,
                request.Query,
                request.Name,
                request.Phone,
                request.Email);

            return (patients, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients for clinic {ClinicId}", clinicId);
            return (Enumerable.Empty<Patient>(), 0);
        }
    }

    public async Task<Patient?> CreatePatientAsync(int clinicId, CreatePatientRequest request)
    {
        try
        {
            var patient = new Patient
            {
                ClinicId = clinicId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var createdPatient = await _patientRepository.CreateAsync(patient);
            _logger.LogInformation("Patient created successfully: {PatientName} for clinic {ClinicId}", 
                createdPatient.Name, clinicId);
            
            return createdPatient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient: {PatientName} for clinic {ClinicId}", 
                request.Name, clinicId);
            return null;
        }
    }

    public async Task<Patient?> UpdatePatientAsync(int id, UpdatePatientRequest request)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found for update: {PatientId}", id);
                return null;
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                patient.Name = request.Name;
            }

            if (request.Phone != null)
            {
                patient.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone;
            }

            if (request.Email != null)
            {
                patient.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
            }

            if (request.Notes != null)
            {
                patient.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes;
            }

            var updatedPatient = await _patientRepository.UpdateAsync(patient);
            _logger.LogInformation("Patient updated successfully: {PatientId}", id);
            
            return updatedPatient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient: {PatientId}", id);
            return null;
        }
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        try
        {
            var result = await _patientRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Patient deleted successfully: {PatientId}", id);
            }
            else
            {
                _logger.LogWarning("Patient not found for deletion: {PatientId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient: {PatientId}", id);
            return false;
        }
    }

    public async Task<bool> PatientExistsInClinicAsync(int id, int clinicId)
    {
        return await _patientRepository.ExistsInClinicAsync(id, clinicId);
    }
}