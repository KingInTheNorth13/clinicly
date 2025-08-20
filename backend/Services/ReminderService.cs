using Hangfire;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;
using backend.Services;

namespace ClinicAppointmentSystem.Services;

public class ReminderService : IReminderService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReminderService> _logger;
    private readonly IBackgroundJobService _backgroundJobService;

    public ReminderService(
        IAppointmentRepository appointmentRepository,
        INotificationService notificationService,
        ILogger<ReminderService> logger,
        IBackgroundJobService backgroundJobService)
    {
        _appointmentRepository = appointmentRepository;
        _notificationService = notificationService;
        _logger = logger;
        _backgroundJobService = backgroundJobService;
    }

    public Task<string> ScheduleReminderAsync(Appointment appointment)
    {
        try
        {
            // Calculate reminder time (24 hours before appointment)
            var reminderTime = appointment.DateTime.AddHours(-24);
            
            // Only schedule if reminder time is in the future
            if (reminderTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Cannot schedule reminder for appointment {AppointmentId}: reminder time {ReminderTime} is in the past", 
                    appointment.Id, reminderTime);
                return Task.FromResult(string.Empty);
            }

            // Schedule the background job
            var jobId = _backgroundJobService.Schedule<IReminderService>(
                service => service.ProcessReminderAsync(appointment.Id),
                reminderTime);

            _logger.LogInformation("Scheduled reminder job {JobId} for appointment {AppointmentId} at {ReminderTime}", 
                jobId, appointment.Id, reminderTime);

            return Task.FromResult(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling reminder for appointment {AppointmentId}", appointment.Id);
            return Task.FromResult(string.Empty);
        }
    }

    public Task<bool> CancelReminderAsync(string jobId)
    {
        try
        {
            if (string.IsNullOrEmpty(jobId))
            {
                _logger.LogWarning("Cannot cancel reminder: job ID is null or empty");
                return Task.FromResult(false);
            }

            var result = _backgroundJobService.Delete(jobId);
            
            if (result)
            {
                _logger.LogInformation("Successfully cancelled reminder job {JobId}", jobId);
            }
            else
            {
                _logger.LogWarning("Failed to cancel reminder job {JobId} - job may not exist or already processed", jobId);
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reminder job {JobId}", jobId);
            return Task.FromResult(false);
        }
    }

    public async Task<string> RescheduleReminderAsync(string oldJobId, Appointment appointment)
    {
        try
        {
            // Cancel the old job
            await CancelReminderAsync(oldJobId);

            // Schedule a new job
            var newJobId = await ScheduleReminderAsync(appointment);

            _logger.LogInformation("Rescheduled reminder from job {OldJobId} to job {NewJobId} for appointment {AppointmentId}", 
                oldJobId, newJobId, appointment.Id);

            return newJobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling reminder for appointment {AppointmentId}", appointment.Id);
            return string.Empty;
        }
    }

    public async Task ProcessReminderAsync(int appointmentId)
    {
        try
        {
            _logger.LogInformation("Processing reminder for appointment {AppointmentId}", appointmentId);

            // Get the appointment with full details
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointment == null)
            {
                _logger.LogWarning("Cannot process reminder: appointment {AppointmentId} not found", appointmentId);
                return;
            }

            // Check if appointment is still scheduled (not cancelled or completed)
            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                _logger.LogInformation("Skipping reminder for appointment {AppointmentId}: status is {Status}", 
                    appointmentId, appointment.Status);
                return;
            }

            // Check if appointment is still in the future
            if (appointment.DateTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Skipping reminder for appointment {AppointmentId}: appointment time {DateTime} has passed", 
                    appointmentId, appointment.DateTime);
                return;
            }

            // Send the reminder notification
            var result = await _notificationService.SendAppointmentReminderAsync(appointment);
            
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to send reminder for appointment {AppointmentId}: {ErrorMessage}", 
                    appointmentId, result.ErrorMessage);
                throw new InvalidOperationException($"Failed to send reminder: {result.ErrorMessage}");
            }

            _logger.LogInformation("Successfully processed reminder for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminder for appointment {AppointmentId}", appointmentId);
            
            // Re-throw the exception so Hangfire can handle retry logic
            throw;
        }
    }
}