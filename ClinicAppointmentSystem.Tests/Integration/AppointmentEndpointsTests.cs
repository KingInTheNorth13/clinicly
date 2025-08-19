using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Endpoints;

namespace ClinicAppointmentSystem.Tests.Integration;

public class AppointmentEndpointsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppointmentEndpointsTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        "Test", options => { });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task SearchAppointments_AsDoctor_ShouldReturnDoctorAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AppointmentResponse>>();
        Assert.NotNull(result);
        Assert.All(result.Items, a => Assert.Equal(1, a.DoctorId)); // Doctor 1's appointments only
    }

    [Fact]
    public async Task SearchAppointments_AsAdmin_ShouldReturnAllAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AppointmentResponse>>();
        Assert.NotNull(result);
        // Admin can see appointments from multiple doctors
    }

    [Fact]
    public async Task SearchAppointments_WithInvalidPageSize_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 200 // Invalid page size
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentById_AsDoctor_WithOwnAppointment_ShouldReturnAppointment()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/appointments/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDetailsResponse>();
        Assert.NotNull(appointment);
        Assert.Equal(1, appointment.Id);
        Assert.Equal(1, appointment.DoctorId);
    }

    [Fact]
    public async Task GetAppointmentById_AsDoctor_WithOtherDoctorAppointment_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act - Try to access appointment that belongs to another doctor
        var response = await _client.GetAsync("/api/appointments/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentById_AsAdmin_ShouldReturnAnyAppointment()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        // Act
        var response = await _client.GetAsync("/api/appointments/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDetailsResponse>();
        Assert.NotNull(appointment);
        Assert.Equal(1, appointment.Id);
    }

    [Fact]
    public async Task CheckAppointmentConflict_AsDoctor_WithNoConflict_ShouldReturnNoConflict()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var conflictRequest = new CheckConflictRequest
        {
            DateTime = DateTime.UtcNow.AddDays(1).AddHours(10) // Tomorrow at 10 AM
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/check-conflict", conflictRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<AppointmentConflictResponse>();
        Assert.NotNull(result);
        Assert.False(result.HasConflict);
        Assert.Empty(result.ConflictingAppointments);
    }

    [Fact]
    public async Task CheckAppointmentConflict_AsAdmin_WithoutDoctorId_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        var conflictRequest = new CheckConflictRequest
        {
            DateTime = DateTime.UtcNow.AddDays(1).AddHours(10)
            // Missing DoctorId for admin
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/check-conflict", conflictRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_AsDoctor_WithValidRequest_ShouldCreateAppointment()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var createRequest = new CreateAppointmentRequest
        {
            PatientId = 1,
            DateTime = DateTime.UtcNow.AddDays(1).AddHours(14), // Tomorrow at 2 PM
            Notes = "Regular checkup"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDetailsResponse>();
        Assert.NotNull(appointment);
        Assert.Equal(1, appointment.PatientId);
        Assert.Equal(1, appointment.DoctorId);
        Assert.Equal("Regular checkup", appointment.Notes);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
    }

    [Fact]
    public async Task CreateAppointment_WithInvalidPatientId_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var createRequest = new CreateAppointmentRequest
        {
            PatientId = 0, // Invalid patient ID
            DateTime = DateTime.UtcNow.AddDays(1).AddHours(14),
            Notes = "Regular checkup"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_WithPastDateTime_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var createRequest = new CreateAppointmentRequest
        {
            PatientId = 1,
            DateTime = DateTime.UtcNow.AddDays(-1), // Past date
            Notes = "Regular checkup"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAppointment_AsDoctor_WithOwnAppointment_ShouldUpdateAppointment()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var updateRequest = new UpdateAppointmentRequest
        {
            Status = AppointmentStatus.Completed,
            Notes = "Updated notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/appointments/1", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDetailsResponse>();
        Assert.NotNull(appointment);
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.Equal("Updated notes", appointment.Notes);
    }

    [Fact]
    public async Task UpdateAppointment_AsDoctor_WithOtherDoctorAppointment_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var updateRequest = new UpdateAppointmentRequest
        {
            Status = AppointmentStatus.Completed
        };

        // Act - Try to update appointment that belongs to another doctor
        var response = await _client.PutAsJsonAsync("/api/appointments/999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAppointment_AsDoctor_WithOwnAppointment_ShouldDeleteAppointment()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.DeleteAsync("/api/appointments/1");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify appointment is deleted
        var getResponse = await _client.GetAsync("/api/appointments/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAppointment_AsDoctor_WithOtherDoctorAppointment_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act - Try to delete appointment that belongs to another doctor
        var response = await _client.DeleteAsync("/api/appointments/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_AsDoctor_WithOwnId_ShouldReturnAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/appointments/doctor/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointments = await response.Content.ReadFromJsonAsync<IEnumerable<AppointmentResponse>>();
        Assert.NotNull(appointments);
        Assert.All(appointments, a => Assert.Equal(1, a.DoctorId));
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_AsDoctor_WithOtherId_ShouldReturn403()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act - Try to get appointments for another doctor
        var response = await _client.GetAsync("/api/appointments/doctor/2");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_AsAdmin_ShouldReturnAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        // Act
        var response = await _client.GetAsync("/api/appointments/doctor/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointments = await response.Content.ReadFromJsonAsync<IEnumerable<AppointmentResponse>>();
        Assert.NotNull(appointments);
        Assert.All(appointments, a => Assert.Equal(1, a.DoctorId));
    }

    [Fact]
    public async Task GetAppointmentsByPatient_WithValidPatientId_ShouldReturnAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/appointments/patient/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var appointments = await response.Content.ReadFromJsonAsync<IEnumerable<AppointmentResponse>>();
        Assert.NotNull(appointments);
        Assert.All(appointments, a => Assert.Equal(1, a.PatientId));
    }

    [Fact]
    public async Task GetAppointmentsByPatient_WithInvalidPatientId_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/appointments/patient/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchAllAppointments_AsAdmin_ShouldReturnAllAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/admin/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AppointmentResponse>>();
        Assert.NotNull(result);
        // Admin can see all appointments regardless of doctor
    }

    [Fact]
    public async Task SearchAllAppointments_AsDoctor_ShouldReturn403()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/admin/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SearchAppointments_Unauthorized_ShouldReturn401()
    {
        // Arrange - No authorization header

        var searchRequest = new AppointmentSearchRequest
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}