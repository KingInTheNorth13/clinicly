using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Hangfire;
using ClinicAppointmentSystem.Services;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;
using backend.Services;

namespace ClinicAppointmentSystem.Tests.Services;

public class ReminderServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<ReminderService>> _mockLogger;
    private readonly Mock<IBackgroundJobService> _mockBackgroundJobService;
    private readonly ReminderService _reminderService;

    public ReminderServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<ReminderService>>();
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();

        _reminderService = new ReminderService(
            _mockAppointmentRepository.Object,
            _mockNotificationService.Object,
            _mockLogger.Object,
            _mockBackgroundJobService.Object);
    }

    [Fact]
    public async Task ScheduleReminderAsync_ValidFutureAppointment_ReturnsJobId()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.UtcNow.AddDays(2), // 2 days in the future
            Status = AppointmentStatus.Scheduled
        };
        var expectedJobId = "job-123";

        _mockBackgroundJobService
            .Setup(x => x.Schedule(
                It.IsAny<System.Linq.Expressions.Expression<System.Action<IReminderService>>>(),
                It.IsAny<DateTime>()))
            .Returns(expectedJobId);

        // Act
        var result = await _reminderService.ScheduleReminderAsync(appointment);

        // Assert
        Assert.Equal(expectedJobId, result);
        _mockBackgroundJobService.Verify(x => x.Schedule(
            It.IsAny<System.Linq.Expressions.Expression<System.Action<IReminderService>>>(),
            It.Is<DateTime>(dt => dt == appointment.DateTime.AddHours(-24))), Times.Once);
    }

    [Fact]
    public async Task ScheduleReminderAsync_PastReminderTime_ReturnsEmptyString()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.UtcNow.AddHours(12), // Only 12 hours in future, reminder would be in past
            Status = AppointmentStatus.Scheduled
        };

        // Act
        var result = await _reminderService.ScheduleReminderAsync(appointment);

        // Assert
        Assert.Equal(string.Empty, result);
        _mockBackgroundJobService.Verify(x => x.Schedule(
            It.IsAny<System.Linq.Expressions.Expression<System.Action<IReminderService>>>(),
            It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task CancelReminderAsync_ValidJobId_ReturnsTrue()
    {
        // Arrange
        var jobId = "job-123";
        _mockBackgroundJobService.Setup(x => x.Delete(jobId)).Returns(true);

        // Act
        var result = await _reminderService.CancelReminderAsync(jobId);

        // Assert
        Assert.True(result);
        _mockBackgroundJobService.Verify(x => x.Delete(jobId), Times.Once);
    }

    [Fact]
    public async Task CancelReminderAsync_EmptyJobId_ReturnsFalse()
    {
        // Act
        var result = await _reminderService.CancelReminderAsync(string.Empty);

        // Assert
        Assert.False(result);
        _mockBackgroundJobService.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RescheduleReminderAsync_ValidAppointment_ReturnsNewJobId()
    {
        // Arrange
        var oldJobId = "old-job-123";
        var newJobId = "new-job-456";
        var appointment = new Appointment
        {
            Id = 1,
            DateTime = DateTime.UtcNow.AddDays(3),
            Status = AppointmentStatus.Scheduled
        };

        _mockBackgroundJobService.Setup(x => x.Delete(oldJobId)).Returns(true);
        _mockBackgroundJobService
            .Setup(x => x.Schedule(
                It.IsAny<System.Linq.Expressions.Expression<System.Action<IReminderService>>>(),
                It.IsAny<DateTime>()))
            .Returns(newJobId);

        // Act
        var result = await _reminderService.RescheduleReminderAsync(oldJobId, appointment);

        // Assert
        Assert.Equal(newJobId, result);
        _mockBackgroundJobService.Verify(x => x.Delete(oldJobId), Times.Once);
        _mockBackgroundJobService.Verify(x => x.Schedule(
            It.IsAny<System.Linq.Expressions.Expression<System.Action<IReminderService>>>(),
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_ValidScheduledAppointment_SendsNotification()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            DateTime = DateTime.UtcNow.AddHours(2),
            Status = AppointmentStatus.Scheduled,
            Patient = new Patient
            {
                Id = 1,
                Name = "John Doe",
                Phone = "+1234567890",
                Email = "john@example.com"
            },
            Doctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Clinic = new Clinic
                {
                    Id = 1,
                    Name = "Test Clinic",
                    Address = "123 Test St"
                }
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdWithDetailsAsync(appointmentId))
            .ReturnsAsync(appointment);

        _mockNotificationService
            .Setup(x => x.SendAppointmentReminderAsync(appointment))
            .ReturnsAsync(NotificationResult.Success("msg-123", NotificationChannel.Email));

        // Act
        await _reminderService.ProcessReminderAsync(appointmentId);

        // Assert
        _mockNotificationService.Verify(x => x.SendAppointmentReminderAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task ProcessReminderAsync_AppointmentNotFound_DoesNotSendNotification()
    {
        // Arrange
        var appointmentId = 1;
        _mockAppointmentRepository
            .Setup(x => x.GetByIdWithDetailsAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        await _reminderService.ProcessReminderAsync(appointmentId);

        // Assert
        _mockNotificationService.Verify(x => x.SendAppointmentReminderAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReminderAsync_CancelledAppointment_DoesNotSendNotification()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            DateTime = DateTime.UtcNow.AddHours(2),
            Status = AppointmentStatus.Cancelled
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdWithDetailsAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        await _reminderService.ProcessReminderAsync(appointmentId);

        // Assert
        _mockNotificationService.Verify(x => x.SendAppointmentReminderAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReminderAsync_PastAppointment_DoesNotSendNotification()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            DateTime = DateTime.UtcNow.AddHours(-1), // Past appointment
            Status = AppointmentStatus.Scheduled
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdWithDetailsAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        await _reminderService.ProcessReminderAsync(appointmentId);

        // Assert
        _mockNotificationService.Verify(x => x.SendAppointmentReminderAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task ProcessReminderAsync_NotificationFails_ThrowsException()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment
        {
            Id = appointmentId,
            DateTime = DateTime.UtcNow.AddHours(2),
            Status = AppointmentStatus.Scheduled,
            Patient = new Patient
            {
                Id = 1,
                Name = "John Doe",
                Phone = "+1234567890",
                Email = "john@example.com"
            },
            Doctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Smith",
                Clinic = new Clinic
                {
                    Id = 1,
                    Name = "Test Clinic",
                    Address = "123 Test St"
                }
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdWithDetailsAsync(appointmentId))
            .ReturnsAsync(appointment);

        _mockNotificationService
            .Setup(x => x.SendAppointmentReminderAsync(appointment))
            .ReturnsAsync(NotificationResult.Failure("Email service unavailable", NotificationChannel.Email));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reminderService.ProcessReminderAsync(appointmentId));
    }
}