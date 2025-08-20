using ClinicAppointmentSystem.Models;
using backend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ClinicAppointmentSystem.Tests.Services;

public class SendGridEmailServiceTests
{
    private readonly Mock<ISendGridClient> _mockSendGridClient;
    private readonly Mock<ILogger<SendGridEmailService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly SendGridEmailService _emailService;

    public SendGridEmailServiceTests()
    {
        _mockSendGridClient = new Mock<ISendGridClient>();
        _mockLogger = new Mock<ILogger<SendGridEmailService>>();
        
        // Create real configuration for testing
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SendGrid:FromEmail"] = "test@clinic.com",
            ["SendGrid:FromName"] = "Test Clinic"
        });
        _configuration = configurationBuilder.Build();

        _emailService = new SendGridEmailService(_mockSendGridClient.Object, _mockLogger.Object, _configuration);
    }

    [Fact]
    public async Task SendEmailAsync_WithException_ReturnsFailureResult()
    {
        // Arrange
        var toEmail = "patient@example.com";
        var subject = "Test Subject";
        var htmlContent = "<h1>Test HTML</h1>";

        _mockSendGridClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _emailService.SendEmailAsync(toEmail, subject, htmlContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.MessageId);
        Assert.Contains("Exception occurred: Network error", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithException_ReturnsFailureResult()
    {
        // Arrange
        var toEmail = "patient@example.com";
        var patientName = "John Doe";
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.Now.AddDays(1),
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _mockSendGridClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SendGrid service unavailable"));

        // Act
        var result = await _emailService.SendAppointmentReminderAsync(toEmail, patientName, appointment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.MessageId);
        Assert.Contains("Exception occurred", result.ErrorMessage);
    }

    [Theory]
    [InlineData("test-message-id")]
    [InlineData("")]
    public void EmailResult_Success_WithMessageId_ReturnsValidResult(string messageId)
    {
        // Act
        var result = EmailResult.Success(messageId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(messageId, result.MessageId);
        Assert.Null(result.ErrorMessage);
        Assert.True(result.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public void EmailResult_Failure_WithErrorMessage_ReturnsValidResult()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = EmailResult.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.MessageId);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.True(result.SentAt <= DateTime.UtcNow);
    }

    [Fact]
    public void SendGridEmailService_Constructor_WithMissingConfiguration_UsesDefaults()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder().Build();
        
        // Act & Assert - Should not throw, uses default values
        var service = new SendGridEmailService(_mockSendGridClient.Object, _mockLogger.Object, emptyConfig);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithValidAppointment_CallsSendEmailAsync()
    {
        // Arrange
        var toEmail = "patient@example.com";
        var patientName = "John Doe";
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.Now.AddDays(1),
            Notes = "Regular checkup",
            Doctor = new Doctor { Name = "Dr. Smith" }
        };

        _mockSendGridClient.Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception")); // Force exception to test error handling

        // Act
        var result = await _emailService.SendAppointmentReminderAsync(toEmail, patientName, appointment);

        // Assert
        Assert.False(result.IsSuccess);
        _mockSendGridClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}