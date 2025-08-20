using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Tests.Integration;

public class MockBackgroundJobService : IBackgroundJobService
{
    private static int _jobIdCounter = 1;

    public string Schedule<T>(System.Linq.Expressions.Expression<System.Action<T>> methodCall, DateTime enqueueAt)
    {
        // Return a mock job ID for testing
        return $"mock-job-{_jobIdCounter++}";
    }

    public bool Delete(string jobId)
    {
        // Always return true for testing
        return true;
    }
}