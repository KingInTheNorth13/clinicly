using ClinicAppointmentSystem.Models;

namespace backend.Services;

public interface INotificationService
{
    Task<NotificationResult> SendAppointmentReminderAsync(Appointment appointment);
    Task<NotificationResult> SendNotificationAsync(NotificationRequest request);
}

public class NotificationRequest
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientPhone { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum NotificationType
{
    AppointmentReminder,
    AppointmentConfirmation,
    AppointmentCancellation,
    AppointmentRescheduled
}

public class NotificationResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public NotificationChannel ChannelUsed { get; set; }
    public DateTime SentAt { get; set; }
    public int RetryCount { get; set; }
    
    public static NotificationResult Success(string messageId, NotificationChannel channel)
    {
        return new NotificationResult
        {
            IsSuccess = true,
            MessageId = messageId,
            ChannelUsed = channel,
            SentAt = DateTime.UtcNow,
            RetryCount = 0
        };
    }
    
    public static NotificationResult Failure(string errorMessage, NotificationChannel attemptedChannel, int retryCount = 0)
    {
        return new NotificationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ChannelUsed = attemptedChannel,
            SentAt = DateTime.UtcNow,
            RetryCount = retryCount
        };
    }
}

public enum NotificationChannel
{
    Email,
    WhatsApp,
    SMS
}