using ClinicAppointmentSystem.Models;
using backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicAppointmentSystem.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        
        // Create configuration for testing
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Notifications:MaxRetryAttempts"] = "2",
            ["Notifications:RetryDelaySeconds"] = "1",
            ["Notifications:PrimaryChannel"] = "Email",
            ["Notifications:FallbackChannel"] = "Email"
        });
        _configuration = configurationBuilder.Build();

        _notificationService = new NotificationService(_mockEmailService.Object, _mockLogger.Object, _configuration);
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithValidAppointment_ReturnsSuccessResult()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.Now.AddDays(1),
            Patient = new Patient { Name = "John Doe", Email = "john@example.com", Phone = "123-456-7890" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _mockEmailService.Setup(e => e.SendAppointmentReminderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Appointment>()))
            .ReturnsAsync(EmailResult.Success("test-message-id"));

        // Act
        var result = await _notificationService.SendAppointmentReminderAsync(appointment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-message-id", result.MessageId);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        Assert.Null(result.ErrorMessage);
        
        _mockEmailService.Verify(e => e.SendAppointmentReminderAsync(
            appointment.Patient.Email, appointment.Patient.Name, appointment), Times.Once);
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithEmailFailure_ReturnsFailureResult()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.Now.AddDays(1),
            Patient = new Patient { Name = "John Doe", Email = "john@example.com" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _mockEmailService.Setup(e => e.SendAppointmentReminderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Appointment>()))
            .ReturnsAsync(EmailResult.Failure("Email service error"));

        // Act
        var result = await _notificationService.SendAppointmentReminderAsync(appointment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email service error", result.ErrorMessage);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        Assert.True(result.RetryCount >= 0);
    }

    [Fact]
    public async Task SendNotificationAsync_WithEmailRequest_CallsEmailService()
    {
        // Arrange
        var request = new NotificationRequest
        {
            RecipientEmail = "test@example.com",
            RecipientName = "Test User",
            Type = NotificationType.AppointmentConfirmation
        };

        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(EmailResult.Success("email-message-id"));

        // Act
        var result = await _notificationService.SendNotificationAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("email-message-id", result.MessageId);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        
        _mockEmailService.Verify(e => e.SendEmailAsync(
            request.RecipientEmail, 
            "Appointment Confirmed", 
            It.IsAny<string>(), 
            null), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_WithMissingEmail_ReturnsFailureResult()
    {
        // Arrange
        var request = new NotificationRequest
        {
            RecipientEmail = "", // Empty email
            RecipientName = "Test User",
            Type = NotificationType.AppointmentConfirmation
        };

        // Act
        var result = await _notificationService.SendNotificationAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Recipient email is required", result.ErrorMessage);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationAsync_WithRetryLogic_RetriesOnFailure()
    {
        // Arrange
        var request = new NotificationRequest
        {
            RecipientEmail = "test@example.com",
            RecipientName = "Test User",
            Type = NotificationType.AppointmentConfirmation
        };

        // Setup to fail first call, succeed on second
        _mockEmailService.SetupSequence(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(EmailResult.Failure("Temporary failure"))
            .ReturnsAsync(EmailResult.Success("retry-success-id"));

        // Act
        var result = await _notificationService.SendNotificationAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("retry-success-id", result.MessageId);
        Assert.Equal(1, result.RetryCount); // Should be 1 because it succeeded on the second attempt (retry count 1)
        
        _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithException_ReturnsFailureResult()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.Now.AddDays(1),
            Patient = new Patient { Name = "John Doe", Email = "john@example.com" },
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _mockEmailService.Setup(e => e.SendAppointmentReminderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Appointment>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _notificationService.SendAppointmentReminderAsync(appointment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Email service exception", result.ErrorMessage);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
    }

    [Theory]
    [InlineData(NotificationType.AppointmentReminder, "Appointment Reminder - Tomorrow")]
    [InlineData(NotificationType.AppointmentConfirmation, "Appointment Confirmed")]
    [InlineData(NotificationType.AppointmentCancellation, "Appointment Cancelled")]
    [InlineData(NotificationType.AppointmentRescheduled, "Appointment Rescheduled")]
    public async Task SendNotificationAsync_WithDifferentTypes_UsesCorrectSubject(NotificationType type, string expectedSubject)
    {
        // Arrange
        var request = new NotificationRequest
        {
            RecipientEmail = "test@example.com",
            RecipientName = "Test User",
            Type = type
        };

        _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(EmailResult.Success("test-id"));

        // Act
        var result = await _notificationService.SendNotificationAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEmailService.Verify(e => e.SendEmailAsync(
            request.RecipientEmail, 
            expectedSubject, 
            It.IsAny<string>(), 
            null), Times.Once);
    }

    [Fact]
    public void NotificationResult_Success_CreatesValidResult()
    {
        // Act
        var result = NotificationResult.Success("test-id", NotificationChannel.Email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-id", result.MessageId);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(0, result.RetryCount);
        Assert.True(result.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public void NotificationResult_Failure_CreatesValidResult()
    {
        // Act
        var result = NotificationResult.Failure("Test error", NotificationChannel.Email, 2);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.MessageId);
        Assert.Equal("Test error", result.ErrorMessage);
        Assert.Equal(NotificationChannel.Email, result.ChannelUsed);
        Assert.Equal(2, result.RetryCount);
        Assert.True(result.SentAt <= DateTime.UtcNow);
    }
}