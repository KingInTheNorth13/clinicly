using ClinicAppointmentSystem.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace backend.Services;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailService(ISendGridClient sendGridClient, ILogger<SendGridEmailService> logger, IConfiguration configuration)
    {
        _sendGridClient = sendGridClient;
        _logger = logger;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@clinic.com";
        _fromName = configuration["SendGrid:FromName"] ?? "Clinic Appointment System";
    }

    public async Task<EmailResult> SendAppointmentReminderAsync(string toEmail, string patientName, Appointment appointment)
    {
        try
        {
            var subject = "Appointment Reminder - Tomorrow";
            var htmlContent = GenerateAppointmentReminderHtml(patientName, appointment);
            var plainTextContent = GenerateAppointmentReminderText(patientName, appointment);

            return await SendEmailAsync(toEmail, subject, htmlContent, plainTextContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment reminder to {Email} for appointment {AppointmentId}", 
                toEmail, appointment.Id);
            return EmailResult.Failure($"Failed to send appointment reminder: {ex.Message}");
        }
    }

    public async Task<EmailResult> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                _logger.LogInformation("Email sent successfully to {Email} with message ID {MessageId}", 
                    toEmail, messageId);
                return EmailResult.Success(messageId ?? "unknown");
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Response: {Response}", 
                    toEmail, response.StatusCode, responseBody);
                return EmailResult.Failure($"SendGrid API error: {response.StatusCode} - {responseBody}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
            return EmailResult.Failure($"Exception occurred: {ex.Message}");
        }
    }

    private string GenerateAppointmentReminderHtml(string patientName, Appointment appointment)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Appointment Reminder</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .appointment-details {{ background-color: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Appointment Reminder</h1>
        </div>
        <div class='content'>
            <p>Dear {patientName},</p>
            <p>This is a friendly reminder about your upcoming appointment.</p>
            
            <div class='appointment-details'>
                <h3>Appointment Details:</h3>
                <p><strong>Date:</strong> {appointment.DateTime:dddd, MMMM dd, yyyy}</p>
                <p><strong>Time:</strong> {appointment.DateTime:h:mm tt}</p>
                <p><strong>Doctor:</strong> {appointment.Doctor?.Name ?? "TBD"}</p>
                {(string.IsNullOrEmpty(appointment.Notes) ? "" : $"<p><strong>Notes:</strong> {appointment.Notes}</p>")}
            </div>
            
            <p>Please arrive 15 minutes early for check-in. If you need to reschedule or cancel, please contact us as soon as possible.</p>
            
            <p>Thank you for choosing our clinic!</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateAppointmentReminderText(string patientName, Appointment appointment)
    {
        return $@"
APPOINTMENT REMINDER

Dear {patientName},

This is a friendly reminder about your upcoming appointment.

Appointment Details:
- Date: {appointment.DateTime:dddd, MMMM dd, yyyy}
- Time: {appointment.DateTime:h:mm tt}
- Doctor: {appointment.Doctor?.Name ?? "TBD"}
{(string.IsNullOrEmpty(appointment.Notes) ? "" : $"- Notes: {appointment.Notes}")}

Please arrive 15 minutes early for check-in. If you need to reschedule or cancel, please contact us as soon as possible.

Thank you for choosing our clinic!

---
This is an automated message. Please do not reply to this email.
";
    }
}