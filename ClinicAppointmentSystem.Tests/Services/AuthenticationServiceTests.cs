using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);

        // Setup configuration
        var configurationData = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", "test-secret-key-that-is-at-least-32-characters-long"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _authService = new AuthenticationService(_context, _configuration, _loggerMock.Object);

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

        var user = new User
        {
            Id = 1,
            ClinicId = 1,
            DoctorId = 1,
            Email = "doctor@test.com",
            PasswordHash = _authService.HashPassword("password123"),
            Role = UserRole.Doctor
        };

        _context.Clinics.Add(clinic);
        _context.Doctors.Add(doctor);
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "testpassword123";

        // Act
        var hashedPassword = _authService.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.True(hashedPassword.Length > 50); // BCrypt hashes are typically 60 characters
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "testpassword123";
        var hashedPassword = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "testpassword123";
        var wrongPassword = "wrongpassword";
        var hashedPassword = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Role = UserRole.Doctor,
            ClinicId = 1,
            DoctorId = 1
        };

        // Act
        var token = _authService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _authService.GenerateRefreshToken();
        var token2 = _authService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "doctor@test.com",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("doctor@test.com", result.User.Email);
        Assert.Equal("Doctor", result.User.Role);
        Assert.Equal(1, result.User.ClinicId);
        Assert.Equal(1, result.User.DoctorId);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "doctor@test.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeTokenAsync_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = "test-refresh-token";

        // Act
        var result = await _authService.RevokeTokenAsync(refreshToken);

        // Assert
        Assert.True(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}