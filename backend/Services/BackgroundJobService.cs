using Hangfire;

namespace ClinicAppointmentSystem.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public BackgroundJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string Schedule<T>(System.Linq.Expressions.Expression<System.Action<T>> methodCall, DateTime enqueueAt)
    {
        return _backgroundJobClient.Schedule(methodCall, enqueueAt);
    }

    public bool Delete(string jobId)
    {
        return _backgroundJobClient.Delete(jobId);
    }
}