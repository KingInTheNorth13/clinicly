using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public interface IReminderService
{
    /// <summary>
    /// Schedules a reminder job for an appointment 24 hours before the appointment time
    /// </summary>
    /// <param name="appointment">The appointment to schedule a reminder for</param>
    /// <returns>The job ID of the scheduled reminder</returns>
    Task<string> ScheduleReminderAsync(Appointment appointment);

    /// <summary>
    /// Cancels a previously scheduled reminder job
    /// </summary>
    /// <param name="jobId">The job ID of the reminder to cancel</param>
    /// <returns>True if the job was successfully cancelled, false otherwise</returns>
    Task<bool> CancelReminderAsync(string jobId);

    /// <summary>
    /// Reschedules a reminder job for an updated appointment
    /// </summary>
    /// <param name="oldJobId">The job ID of the existing reminder to cancel</param>
    /// <param name="appointment">The updated appointment to schedule a new reminder for</param>
    /// <returns>The job ID of the new scheduled reminder</returns>
    Task<string> RescheduleReminderAsync(string oldJobId, Appointment appointment);

    /// <summary>
    /// Processes the reminder delivery for an appointment
    /// This method is called by Hangfire when the scheduled time arrives
    /// </summary>
    /// <param name="appointmentId">The ID of the appointment to send a reminder for</param>
    Task ProcessReminderAsync(int appointmentId);
}