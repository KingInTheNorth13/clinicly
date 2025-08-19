using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using ClinicAppointmentSystem.DTOs;

namespace ClinicAppointmentSystem.Tests.Integration;

public class PatientEndpointsTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PatientEndpointsTests(TestWebApplicationFactory<Program> factory)
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
    public async Task GetPatients_AsDoctor_ShouldReturnPatientsInClinic()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var patients = await response.Content.ReadFromJsonAsync<IEnumerable<PatientResponse>>();
        Assert.NotNull(patients);
        Assert.Equal(2, patients.Count());
        Assert.All(patients, p => Assert.Equal(1, p.ClinicId));
    }

    [Fact]
    public async Task GetPatients_AsAdmin_ShouldReturnPatientsInClinic()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var patients = await response.Content.ReadFromJsonAsync<IEnumerable<PatientResponse>>();
        Assert.NotNull(patients);
        Assert.Equal(2, patients.Count());
    }

    [Fact]
    public async Task GetPatients_Unauthorized_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SearchPatients_WithQuery_ShouldReturnMatchingPatients()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var searchRequest = new PatientSearchRequest
        {
            Query = "John",
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("John Doe", result.Items.First().Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task SearchPatients_WithInvalidPageSize_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var searchRequest = new PatientSearchRequest
        {
            Page = 1,
            PageSize = 200 // Invalid page size
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients/search", searchRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPatientById_WithValidId_ShouldReturnPatient()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/patients/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(patient);
        Assert.Equal(1, patient.Id);
        Assert.Equal("John Doe", patient.Name);
    }

    [Fact]
    public async Task GetPatientById_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/patients/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPatientWithAppointments_WithValidId_ShouldReturnPatientWithAppointments()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.GetAsync("/api/patients/1/appointments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var patient = await response.Content.ReadFromJsonAsync<PatientWithAppointmentsResponse>();
        Assert.NotNull(patient);
        Assert.Equal(1, patient.Id);
        Assert.Equal("John Doe", patient.Name);
        Assert.NotEmpty(patient.Appointments);
    }

    [Fact]
    public async Task CreatePatient_WithValidRequest_ShouldCreatePatient()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var createRequest = new CreatePatientRequest
        {
            Name = "New Patient",
            Phone = "555-123-4567",
            Email = "newpatient@test.com",
            Notes = "New patient notes"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(patient);
        Assert.Equal("New Patient", patient.Name);
        Assert.Equal("555-123-4567", patient.Phone);
        Assert.Equal("newpatient@test.com", patient.Email);
        Assert.Equal("New patient notes", patient.Notes);
    }

    [Fact]
    public async Task CreatePatient_WithEmptyName_ShouldReturn400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var createRequest = new CreatePatientRequest
        {
            Name = "", // Empty name
            Phone = "555-123-4567",
            Email = "newpatient@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePatient_WithValidRequest_ShouldUpdatePatient()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var updateRequest = new UpdatePatientRequest
        {
            Name = "Updated Name",
            Phone = "999-888-7777",
            Email = "updated@test.com",
            Notes = "Updated notes"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/patients/1", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(patient);
        Assert.Equal("Updated Name", patient.Name);
        Assert.Equal("999-888-7777", patient.Phone);
        Assert.Equal("updated@test.com", patient.Email);
        Assert.Equal("Updated notes", patient.Notes);
    }

    [Fact]
    public async Task UpdatePatient_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        var updateRequest = new UpdatePatientRequest
        {
            Name = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/patients/999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePatient_AsAdmin_ShouldDeletePatient()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        // Act
        var response = await _client.DeleteAsync("/api/patients/2");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify patient is deleted
        var getResponse = await _client.GetAsync("/api/patients/2");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePatient_AsDoctor_ShouldReturn403()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Doctor");

        // Act
        var response = await _client.DeleteAsync("/api/patients/2");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeletePatient_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test", "Admin");

        // Act
        var response = await _client.DeleteAsync("/api/patients/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
        }

        var role = authorizationHeader.Replace("Test ", "");
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, role == "Admin" ? "1" : "2"),
            new Claim(ClaimTypes.Email, role == "Admin" ? "admin@testclinic.com" : "doctor@testclinic.com"),
            new Claim(ClaimTypes.Role, role),
            new Claim("ClinicId", "1"),
            new Claim("DoctorId", role == "Doctor" ? "1" : "")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}