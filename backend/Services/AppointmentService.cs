using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;

namespace ClinicAppointmentSystem.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUserService _userService;
    private readonly IReminderService _reminderService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IUserService userService,
        IReminderService reminderService,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _userService = userService;
        _reminderService = reminderService;
        _logger = logger;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdAsync(id);
    }

    public async Task<AppointmentDetailsResponse?> GetAppointmentDetailsAsync(int id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
            if (appointment == null)
                return null;

            return new AppointmentDetailsResponse
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.Name,
                DoctorSpecialization = appointment.Doctor.Specialization,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.Name,
                PatientPhone = appointment.Patient.Phone,
                PatientEmail = appointment.Patient.Email,
                DateTime = appointment.DateTime,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                ClinicName = appointment.Doctor.Clinic.Name,
                ClinicAddress = appointment.Doctor.Clinic.Address
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment details for appointment {AppointmentId}", id);
            return null;
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _appointmentRepository.GetByDoctorIdAsync(doctorId, startDate, endDate);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await _appointmentRepository.GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByClinicIdAsync(int clinicId, DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _appointmentRepository.GetByClinicIdAsync(clinicId, startDate, endDate);
    }

    public async Task<(IEnumerable<AppointmentResponse> Appointments, int TotalCount)> SearchAppointmentsAsync(AppointmentSearchRequest request, int? restrictToDoctorId = null)
    {
        try
        {
            // If restricting to a specific doctor, override the request's doctor filter
            var effectiveDoctorId = restrictToDoctorId ?? request.DoctorId;

            var appointments = await _appointmentRepository.GetAppointmentsAsync(
                effectiveDoctorId,
                request.PatientId,
                null, // clinicId - we'll handle this through doctor filtering
                request.StartDate,
                request.EndDate,
                request.Status,
                request.Page,
                request.PageSize);

            var totalCount = await _appointmentRepository.GetAppointmentCountAsync(
                effectiveDoctorId,
                request.PatientId,
                null,
                request.StartDate,
                request.EndDate,
                request.Status);

            var appointmentResponses = appointments.Select(a => new AppointmentResponse
            {
                Id = a.Id,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.Name,
                PatientId = a.PatientId,
                PatientName = a.Patient.Name,
                PatientPhone = a.Patient.Phone,
                PatientEmail = a.Patient.Email,
                DateTime = a.DateTime,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            });

            return (appointmentResponses, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching appointments");
            return (Enumerable.Empty<AppointmentResponse>(), 0);
        }
    }

    public async Task<AppointmentConflictResponse> CheckAppointmentConflictAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId = null)
    {
        try
        {
            var hasConflict = await _appointmentRepository.HasConflictAsync(doctorId, dateTime, excludeAppointmentId);
            var conflictingAppointments = new List<AppointmentResponse>();

            if (hasConflict)
            {
                var conflicts = await _appointmentRepository.GetConflictingAppointmentsAsync(doctorId, dateTime, excludeAppointmentId);
                conflictingAppointments = conflicts.Select(a => new AppointmentResponse
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = string.Empty, // Not loaded in this query
                    PatientId = a.PatientId,
                    PatientName = a.Patient.Name,
                    PatientPhone = a.Patient.Phone,
                    PatientEmail = a.Patient.Email,
                    DateTime = a.DateTime,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                }).ToList();
            }

            // Generate suggested times (next 3 available slots in 30-minute intervals)
            var suggestedTimes = await GenerateSuggestedTimesAsync(doctorId, dateTime);

            return new AppointmentConflictResponse
            {
                HasConflict = hasConflict,
                ConflictingAppointments = conflictingAppointments,
                SuggestedTimes = suggestedTimes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking appointment conflict for doctor {DoctorId} at {DateTime}", doctorId, dateTime);
            return new AppointmentConflictResponse
            {
                HasConflict = false,
                ConflictingAppointments = new List<AppointmentResponse>(),
                SuggestedTimes = new List<DateTime>()
            };
        }
    }

    public async Task<Appointment?> CreateAppointmentAsync(int doctorId, CreateAppointmentRequest request)
    {
        try
        {
            // Validate patient exists
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found when creating appointment: {PatientId}", request.PatientId);
                return null;
            }

            // Check for conflicts
            var conflictCheck = await CheckAppointmentConflictAsync(doctorId, request.DateTime);
            if (conflictCheck.HasConflict)
            {
                _logger.LogWarning("Appointment conflict detected for doctor {DoctorId} at {DateTime}", doctorId, request.DateTime);
                return null;
            }

            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = request.PatientId,
                DateTime = request.DateTime,
                Status = AppointmentStatus.Scheduled,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            var createdAppointment = await _appointmentRepository.CreateAsync(appointment);
            
            // Schedule reminder job for the new appointment
            try
            {
                var jobId = await _reminderService.ScheduleReminderAsync(createdAppointment);
                if (!string.IsNullOrEmpty(jobId))
                {
                    // Update the appointment with the job ID
                    createdAppointment.ReminderJobId = jobId;
                    await _appointmentRepository.UpdateAsync(createdAppointment);
                    _logger.LogInformation("Scheduled reminder job {JobId} for appointment {AppointmentId}", jobId, createdAppointment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to schedule reminder for appointment {AppointmentId}, but appointment was created successfully", createdAppointment.Id);
            }
            
            _logger.LogInformation("Appointment created successfully: {AppointmentId} for doctor {DoctorId} and patient {PatientId}", 
                createdAppointment.Id, doctorId, request.PatientId);
            
            return createdAppointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment for doctor {DoctorId} and patient {PatientId}", 
                doctorId, request.PatientId);
            return null;
        }
    }

    public async Task<Appointment?> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request, int? restrictToDoctorId = null)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found for update: {AppointmentId}", id);
                return null;
            }

            // Check if access is restricted to a specific doctor
            if (restrictToDoctorId.HasValue && appointment.DoctorId != restrictToDoctorId.Value)
            {
                _logger.LogWarning("Access denied: Doctor {DoctorId} cannot update appointment {AppointmentId} belonging to doctor {AppointmentDoctorId}", 
                    restrictToDoctorId.Value, id, appointment.DoctorId);
                return null;
            }

            // Validate patient if being changed
            if (request.PatientId.HasValue && request.PatientId.Value != appointment.PatientId)
            {
                var patient = await _patientRepository.GetByIdAsync(request.PatientId.Value);
                if (patient == null)
                {
                    _logger.LogWarning("Patient not found when updating appointment: {PatientId}", request.PatientId.Value);
                    return null;
                }
                appointment.PatientId = request.PatientId.Value;
            }

            // Check for conflicts if datetime is being changed
            bool dateTimeChanged = false;
            if (request.DateTime.HasValue && request.DateTime.Value != appointment.DateTime)
            {
                var conflictCheck = await CheckAppointmentConflictAsync(appointment.DoctorId, request.DateTime.Value, id);
                if (conflictCheck.HasConflict)
                {
                    _logger.LogWarning("Appointment conflict detected when updating appointment {AppointmentId} to {DateTime}", 
                        id, request.DateTime.Value);
                    return null;
                }
                appointment.DateTime = request.DateTime.Value;
                dateTimeChanged = true;
            }

            // Update other fields
            if (request.Status.HasValue)
            {
                appointment.Status = request.Status.Value;
            }

            if (request.Notes != null)
            {
                appointment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes;
            }

            var updatedAppointment = await _appointmentRepository.UpdateAsync(appointment);
            
            // Handle reminder rescheduling if needed
            try
            {
                if (dateTimeChanged || (request.Status.HasValue && request.Status.Value == AppointmentStatus.Cancelled))
                {
                    if (!string.IsNullOrEmpty(updatedAppointment.ReminderJobId))
                    {
                        if (request.Status.HasValue && request.Status.Value == AppointmentStatus.Cancelled)
                        {
                            // Cancel reminder for cancelled appointments
                            await _reminderService.CancelReminderAsync(updatedAppointment.ReminderJobId);
                            updatedAppointment.ReminderJobId = null;
                            await _appointmentRepository.UpdateAsync(updatedAppointment);
                            _logger.LogInformation("Cancelled reminder for cancelled appointment {AppointmentId}", id);
                        }
                        else if (dateTimeChanged && updatedAppointment.Status == AppointmentStatus.Scheduled)
                        {
                            // Reschedule reminder for date/time changes
                            var newJobId = await _reminderService.RescheduleReminderAsync(updatedAppointment.ReminderJobId, updatedAppointment);
                            if (!string.IsNullOrEmpty(newJobId))
                            {
                                updatedAppointment.ReminderJobId = newJobId;
                                await _appointmentRepository.UpdateAsync(updatedAppointment);
                                _logger.LogInformation("Rescheduled reminder for appointment {AppointmentId} with new job {JobId}", id, newJobId);
                            }
                        }
                    }
                    else if (dateTimeChanged && updatedAppointment.Status == AppointmentStatus.Scheduled)
                    {
                        // Schedule new reminder if none exists and appointment is scheduled
                        var jobId = await _reminderService.ScheduleReminderAsync(updatedAppointment);
                        if (!string.IsNullOrEmpty(jobId))
                        {
                            updatedAppointment.ReminderJobId = jobId;
                            await _appointmentRepository.UpdateAsync(updatedAppointment);
                            _logger.LogInformation("Scheduled new reminder for appointment {AppointmentId} with job {JobId}", id, jobId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle reminder scheduling for updated appointment {AppointmentId}", id);
            }
            
            _logger.LogInformation("Appointment updated successfully: {AppointmentId}", id);
            
            return updatedAppointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment: {AppointmentId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id, int? restrictToDoctorId = null)
    {
        try
        {
            // Get appointment first to check access and cancel reminder
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found for deletion: {AppointmentId}", id);
                return false;
            }

            // Check if access is restricted to a specific doctor
            if (restrictToDoctorId.HasValue && appointment.DoctorId != restrictToDoctorId.Value)
            {
                _logger.LogWarning("Access denied: Doctor {DoctorId} cannot delete appointment {AppointmentId} belonging to doctor {AppointmentDoctorId}", 
                    restrictToDoctorId.Value, id, appointment.DoctorId);
                return false;
            }

            // Cancel reminder job if it exists
            if (!string.IsNullOrEmpty(appointment.ReminderJobId))
            {
                try
                {
                    await _reminderService.CancelReminderAsync(appointment.ReminderJobId);
                    _logger.LogInformation("Cancelled reminder job {JobId} for deleted appointment {AppointmentId}", 
                        appointment.ReminderJobId, id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel reminder job {JobId} for appointment {AppointmentId}", 
                        appointment.ReminderJobId, id);
                }
            }

            var result = await _appointmentRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("Appointment deleted successfully: {AppointmentId}", id);
            }
            else
            {
                _logger.LogWarning("Appointment not found for deletion: {AppointmentId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment: {AppointmentId}", id);
            return false;
        }
    }

    public async Task<bool> AppointmentExistsForDoctorAsync(int id, int doctorId)
    {
        return await _appointmentRepository.ExistsForDoctorAsync(id, doctorId);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(DateTime fromDate, int? doctorId = null)
    {
        return await _appointmentRepository.GetUpcomingAppointmentsAsync(fromDate, doctorId);
    }

    public async Task<bool> ValidateAppointmentAccessAsync(int appointmentId, int userId, UserRole userRole)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return false;

            // Admins can access all appointments
            if (userRole == UserRole.Admin)
                return true;

            // Doctors can only access their own appointments
            if (userRole == UserRole.Doctor)
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user?.DoctorId.HasValue == true)
                {
                    return appointment.DoctorId == user.DoctorId.Value;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating appointment access for user {UserId} and appointment {AppointmentId}", userId, appointmentId);
            return false;
        }
    }

    private async Task<IEnumerable<DateTime>> GenerateSuggestedTimesAsync(int doctorId, DateTime requestedDateTime)
    {
        var suggestedTimes = new List<DateTime>();
        var currentTime = requestedDateTime;
        var maxSuggestions = 3;
        var intervalMinutes = 30;
        var maxHoursToCheck = 8; // Check up to 8 hours ahead

        for (int i = 0; i < maxHoursToCheck * 2 && suggestedTimes.Count < maxSuggestions; i++)
        {
            currentTime = currentTime.AddMinutes(intervalMinutes);
            
            // Skip times outside business hours (8 AM to 6 PM)
            if (currentTime.Hour < 8 || currentTime.Hour >= 18)
            {
                // Jump to next day at 8 AM if we're past business hours
                if (currentTime.Hour >= 18)
                {
                    currentTime = currentTime.Date.AddDays(1).AddHours(8);
                }
                else
                {
                    currentTime = currentTime.Date.AddHours(8);
                }
            }

            // Skip weekends
            if (currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
            {
                // Jump to Monday 8 AM
                var daysToAdd = currentTime.DayOfWeek == DayOfWeek.Saturday ? 2 : 1;
                currentTime = currentTime.Date.AddDays(daysToAdd).AddHours(8);
            }

            var hasConflict = await _appointmentRepository.HasConflictAsync(doctorId, currentTime);
            if (!hasConflict)
            {
                suggestedTimes.Add(currentTime);
            }
        }

        return suggestedTimes;
    }
}