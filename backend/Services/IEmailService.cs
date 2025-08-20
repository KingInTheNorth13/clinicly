using ClinicAppointmentSystem.Models;

namespace backend.Services;

public interface IEmailService
{
    Task<EmailResult> SendAppointmentReminderAsync(string toEmail, string patientName, Appointment appointment);
    Task<EmailResult> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null);
}

public class EmailResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    
    public static EmailResult Success(string messageId)
    {
        return new EmailResult
        {
            IsSuccess = true,
            MessageId = messageId,
            SentAt = DateTime.UtcNow
        };
    }
    
    public static EmailResult Failure(string errorMessage)
    {
        return new EmailResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            SentAt = DateTime.UtcNow
        };
    }
}