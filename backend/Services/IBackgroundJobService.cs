namespace ClinicAppointmentSystem.Services;

public interface IBackgroundJobService
{
    /// <summary>
    /// Schedules a background job to be executed at a specific time
    /// </summary>
    /// <param name="methodCall">The method to execute</param>
    /// <param name="enqueueAt">When to execute the job</param>
    /// <returns>The job ID</returns>
    string Schedule<T>(System.Linq.Expressions.Expression<System.Action<T>> methodCall, DateTime enqueueAt);

    /// <summary>
    /// Deletes a scheduled job
    /// </summary>
    /// <param name="jobId">The job ID to delete</param>
    /// <returns>True if the job was deleted, false otherwise</returns>
    bool Delete(string jobId);
}