using ClinicAppointmentSystem.Models;

namespace backend.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationSettings _settings;

    public NotificationService(
        IEmailService emailService,
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _logger = logger;
        _settings = new NotificationSettings
        {
            MaxRetryAttempts = configuration.GetValue<int>("Notifications:MaxRetryAttempts", 3),
            RetryDelaySeconds = configuration.GetValue<int>("Notifications:RetryDelaySeconds", 5),
            PrimaryChannel = Enum.Parse<NotificationChannel>(
                configuration.GetValue<string>("Notifications:PrimaryChannel") ?? "Email", true),
            FallbackChannel = Enum.Parse<NotificationChannel>(
                configuration.GetValue<string>("Notifications:FallbackChannel") ?? "Email", true)
        };
    }

    public async Task<NotificationResult> SendAppointmentReminderAsync(Appointment appointment)
    {
        try
        {
            var request = new NotificationRequest
            {
                RecipientEmail = appointment.Patient.Email ?? string.Empty,
                RecipientPhone = appointment.Patient.Phone,
                RecipientName = appointment.Patient.Name,
                Type = NotificationType.AppointmentReminder,
                Data = new Dictionary<string, object>
                {
                    ["appointment"] = appointment,
                    ["patientName"] = appointment.Patient.Name,
                    ["doctorName"] = appointment.Doctor?.Name ?? "TBD",
                    ["appointmentDate"] = appointment.DateTime,
                    ["appointmentId"] = appointment.Id
                }
            };

            return await SendNotificationAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment reminder for appointment {AppointmentId}", appointment.Id);
            return NotificationResult.Failure($"Failed to send appointment reminder: {ex.Message}", NotificationChannel.Email);
        }
    }

    public async Task<NotificationResult> SendNotificationAsync(NotificationRequest request)
    {
        var result = await SendWithRetryAsync(request, _settings.PrimaryChannel);
        
        if (!result.IsSuccess && _settings.FallbackChannel != _settings.PrimaryChannel)
        {
            _logger.LogWarning("Primary channel {PrimaryChannel} failed for {RecipientEmail}, trying fallback {FallbackChannel}",
                _settings.PrimaryChannel, request.RecipientEmail, _settings.FallbackChannel);
            
            result = await SendWithRetryAsync(request, _settings.FallbackChannel);
        }

        return result;
    }

    private async Task<NotificationResult> SendWithRetryAsync(NotificationRequest request, NotificationChannel channel)
    {
        NotificationResult? lastResult = null;
        
        for (int attempt = 0; attempt <= _settings.MaxRetryAttempts; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelaySeconds * Math.Pow(2, attempt - 1)); // Exponential backoff
                    _logger.LogInformation("Retrying notification attempt {Attempt} after {Delay}ms delay", 
                        attempt + 1, delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }

                lastResult = await SendViaChannelAsync(request, channel);
                
                if (lastResult.IsSuccess)
                {
                    lastResult.RetryCount = attempt;
                    _logger.LogInformation("Notification sent successfully via {Channel} on attempt {Attempt} to {Recipient}",
                        channel, attempt + 1, request.RecipientEmail);
                    return lastResult;
                }

                _logger.LogWarning("Notification attempt {Attempt} failed via {Channel} for {Recipient}: {Error}",
                    attempt + 1, channel, request.RecipientEmail, lastResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during notification attempt {Attempt} via {Channel} for {Recipient}",
                    attempt + 1, channel, request.RecipientEmail);
                
                lastResult = NotificationResult.Failure($"Exception on attempt {attempt + 1}: {ex.Message}", channel, attempt);
            }
        }

        _logger.LogError("All notification attempts failed for {Recipient} via {Channel} after {MaxAttempts} attempts",
            request.RecipientEmail, channel, _settings.MaxRetryAttempts + 1);
        
        return lastResult ?? NotificationResult.Failure("All retry attempts failed", channel, _settings.MaxRetryAttempts);
    }

    private async Task<NotificationResult> SendViaChannelAsync(NotificationRequest request, NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => await SendViaEmailAsync(request),
            NotificationChannel.WhatsApp => await SendViaWhatsAppAsync(request),
            NotificationChannel.SMS => await SendViaSmsAsync(request),
            _ => NotificationResult.Failure($"Unsupported notification channel: {channel}", channel)
        };
    }

    private async Task<NotificationResult> SendViaEmailAsync(NotificationRequest request)
    {
        if (string.IsNullOrEmpty(request.RecipientEmail))
        {
            return NotificationResult.Failure("Recipient email is required for email notifications", NotificationChannel.Email);
        }

        try
        {
            EmailResult emailResult;
            
            if (request.Type == NotificationType.AppointmentReminder && request.Data.TryGetValue("appointment", out var appointmentObj))
            {
                var appointment = (Appointment)appointmentObj;
                emailResult = await _emailService.SendAppointmentReminderAsync(request.RecipientEmail, request.RecipientName, appointment);
            }
            else
            {
                // For other notification types, use generic email sending
                var subject = GetEmailSubject(request.Type);
                var htmlContent = GenerateEmailContent(request);
                emailResult = await _emailService.SendEmailAsync(request.RecipientEmail, subject, htmlContent);
            }

            if (emailResult.IsSuccess)
            {
                return NotificationResult.Success(emailResult.MessageId ?? "unknown", NotificationChannel.Email);
            }
            else
            {
                return NotificationResult.Failure(emailResult.ErrorMessage ?? "Email sending failed", NotificationChannel.Email);
            }
        }
        catch (Exception ex)
        {
            return NotificationResult.Failure($"Email service exception: {ex.Message}", NotificationChannel.Email);
        }
    }

    private async Task<NotificationResult> SendViaWhatsAppAsync(NotificationRequest request)
    {
        // Placeholder for future WhatsApp implementation
        await Task.CompletedTask;
        return NotificationResult.Failure("WhatsApp notifications not yet implemented", NotificationChannel.WhatsApp);
    }

    private async Task<NotificationResult> SendViaSmsAsync(NotificationRequest request)
    {
        // Placeholder for future SMS implementation
        await Task.CompletedTask;
        return NotificationResult.Failure("SMS notifications not yet implemented", NotificationChannel.SMS);
    }

    private string GetEmailSubject(NotificationType type)
    {
        return type switch
        {
            NotificationType.AppointmentReminder => "Appointment Reminder - Tomorrow",
            NotificationType.AppointmentConfirmation => "Appointment Confirmed",
            NotificationType.AppointmentCancellation => "Appointment Cancelled",
            NotificationType.AppointmentRescheduled => "Appointment Rescheduled",
            _ => "Clinic Notification"
        };
    }

    private string GenerateEmailContent(NotificationRequest request)
    {
        // Basic HTML template for non-reminder notifications
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Clinic Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{GetEmailSubject(request.Type)}</h1>
        </div>
        <div class='content'>
            <p>Dear {request.RecipientName},</p>
            <p>This is a notification regarding your appointment.</p>
            <p>If you have any questions, please contact our clinic.</p>
            <p>Thank you for choosing our clinic!</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class NotificationSettings
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public NotificationChannel PrimaryChannel { get; set; } = NotificationChannel.Email;
    public NotificationChannel FallbackChannel { get; set; } = NotificationChannel.Email;
}