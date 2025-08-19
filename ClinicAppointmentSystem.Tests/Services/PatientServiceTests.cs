using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Tests.Services;

public class PatientServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<PatientService>> _loggerMock;
    private readonly PatientRepository _patientRepository;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _patientRepository = new PatientRepository(_context);
        
        _loggerMock = new Mock<ILogger<PatientService>>();
        
        _patientService = new PatientService(_patientRepository, _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var clinic = new Clinic
        {
            Id = 1,
            Name = "Test Clinic",
            Address = "123 Test St"
        };

        var doctor = new Doctor
        {
            Id = 1,
            ClinicId = 1,
            Name = "Dr. Test",
            Specialization = "General",
            Email = "doctor@test.com"
        };

        var patient1 = new Patient
        {
            Id = 1,
            ClinicId = 1,
            Name = "John Doe",
            Phone = "123-456-7890",
            Email = "john@test.com",
            Notes = "Test patient 1"
        };

        var patient2 = new Patient
        {
            Id = 2,
            ClinicId = 1,
            Name = "Jane Smith",
            Phone = "098-765-4321",
            Email = "jane@test.com",
            Notes = "Test patient 2"
        };

        var appointment = new Appointment
        {
            Id = 1,
            DoctorId = 1,
            PatientId = 1,
            DateTime = DateTime.UtcNow.AddDays(1),
            Status = AppointmentStatus.Scheduled,
            Notes = "Test appointment"
        };

        _context.Clinics.Add(clinic);
        _context.Doctors.Add(doctor);
        _context.Patients.AddRange(patient1, patient2);
        _context.Appointments.Add(appointment);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetPatientByIdAsync_WithValidId_ShouldReturnPatient()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John Doe", result.Name);
    }

    [Fact]
    public async Task GetPatientByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPatientWithAppointmentsAsync_WithValidId_ShouldReturnPatientWithAppointments()
    {
        // Act
        var result = await _patientService.GetPatientWithAppointmentsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.NotEmpty(result.Appointments);
        Assert.Single(result.Appointments);
    }

    [Fact]
    public async Task GetPatientsByClinicIdAsync_ShouldReturnPatientsInClinic()
    {
        // Act
        var result = await _patientService.GetPatientsByClinicIdAsync(1);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(1, p.ClinicId));
    }

    [Fact]
    public async Task SearchPatientsAsync_WithQuery_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest
        {
            Query = "John",
            Page = 1,
            PageSize = 10
        };

        // Act
        var (patients, totalCount) = await _patientService.SearchPatientsAsync(1, searchRequest);

        // Assert
        Assert.Single(patients);
        Assert.Equal(1, totalCount);
        Assert.Equal("John Doe", patients.First().Name);
    }

    [Fact]
    public async Task SearchPatientsAsync_WithNameFilter_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest
        {
            Name = "Jane",
            Page = 1,
            PageSize = 10
        };

        // Act
        var (patients, totalCount) = await _patientService.SearchPatientsAsync(1, searchRequest);

        // Assert
        Assert.Single(patients);
        Assert.Equal(1, totalCount);
        Assert.Equal("Jane Smith", patients.First().Name);
    }

    [Fact]
    public async Task SearchPatientsAsync_WithPhoneFilter_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest
        {
            Phone = "123-456",
            Page = 1,
            PageSize = 10
        };

        // Act
        var (patients, totalCount) = await _patientService.SearchPatientsAsync(1, searchRequest);

        // Assert
        Assert.Single(patients);
        Assert.Equal(1, totalCount);
        Assert.Equal("John Doe", patients.First().Name);
    }

    [Fact]
    public async Task SearchPatientsAsync_WithEmailFilter_ShouldReturnMatchingPatients()
    {
        // Arrange
        var searchRequest = new PatientSearchRequest
        {
            Email = "jane@test.com",
            Page = 1,
            PageSize = 10
        };

        // Act
        var (patients, totalCount) = await _patientService.SearchPatientsAsync(1, searchRequest);

        // Assert
        Assert.Single(patients);
        Assert.Equal(1, totalCount);
        Assert.Equal("Jane Smith", patients.First().Name);
    }

    [Fact]
    public async Task CreatePatientAsync_WithValidRequest_ShouldCreatePatient()
    {
        // Arrange
        var request = new CreatePatientRequest
        {
            Name = "New Patient",
            Phone = "555-123-4567",
            Email = "newpatient@test.com",
            Notes = "New patient notes"
        };

        // Act
        var result = await _patientService.CreatePatientAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Patient", result.Name);
        Assert.Equal("555-123-4567", result.Phone);
        Assert.Equal("newpatient@test.com", result.Email);
        Assert.Equal("New patient notes", result.Notes);
        Assert.Equal(1, result.ClinicId);
    }

    [Fact]
    public async Task UpdatePatientAsync_WithValidRequest_ShouldUpdatePatient()
    {
        // Arrange
        var request = new UpdatePatientRequest
        {
            Name = "Updated Name",
            Phone = "999-888-7777",
            Email = "updated@test.com",
            Notes = "Updated notes"
        };

        // Act
        var result = await _patientService.UpdatePatientAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("999-888-7777", result.Phone);
        Assert.Equal("updated@test.com", result.Email);
        Assert.Equal("Updated notes", result.Notes);
    }

    [Fact]
    public async Task UpdatePatientAsync_WithNonExistentPatient_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdatePatientRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _patientService.UpdatePatientAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeletePatientAsync_WithValidId_ShouldReturnTrue()
    {
        // Act
        var result = await _patientService.DeletePatientAsync(2); // Delete patient without appointments

        // Assert
        Assert.True(result);
        
        // Verify patient is deleted
        var deletedPatient = await _patientService.GetPatientByIdAsync(2);
        Assert.Null(deletedPatient);
    }

    [Fact]
    public async Task DeletePatientAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _patientService.DeletePatientAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PatientExistsInClinicAsync_WithValidPatientAndClinic_ShouldReturnTrue()
    {
        // Act
        var result = await _patientService.PatientExistsInClinicAsync(1, 1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PatientExistsInClinicAsync_WithInvalidPatient_ShouldReturnFalse()
    {
        // Act
        var result = await _patientService.PatientExistsInClinicAsync(999, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PatientExistsInClinicAsync_WithWrongClinic_ShouldReturnFalse()
    {
        // Act
        var result = await _patientService.PatientExistsInClinicAsync(1, 999);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}