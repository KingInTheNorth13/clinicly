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

public class UserServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserRepository _userRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _userRepository = new UserRepository(_context);
        
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        
        _userService = new UserService(_userRepository, _authServiceMock.Object, _loggerMock.Object);

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
            PasswordHash = "hashedpassword",
            Role = UserRole.Doctor
        };

        _context.Clinics.Add(clinic);
        _context.Doctors.Add(doctor);
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Act
        var result = await _userService.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("doctor@test.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _userService.GetUserByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Act
        var result = await _userService.GetUserByEmailAsync("doctor@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("doctor@test.com", result.Email);
    }

    [Fact]
    public async Task GetUsersByClinicIdAsync_ShouldReturnUsersInClinic()
    {
        // Act
        var result = await _userService.GetUsersByClinicIdAsync(1);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, u => Assert.Equal(1, u.ClinicId));
    }

    [Fact]
    public async Task CreateUserAsync_WithValidRequest_ShouldCreateUser()
    {
        // Arrange
        _authServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedpassword");

        var request = new CreateUserRequest
        {
            Email = "newuser@test.com",
            Password = "password123",
            Role = "Admin",
            ClinicId = 1
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
        Assert.Equal(UserRole.Admin, result.Role);
        Assert.Equal(1, result.ClinicId);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "doctor@test.com", // This email already exists
            Password = "password123",
            Role = "Doctor",
            ClinicId = 1
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidRole_ShouldReturnNull()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "newuser@test.com",
            Password = "password123",
            Role = "InvalidRole",
            ClinicId = 1
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidRequest_ShouldUpdateUser()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "updated@test.com",
            Role = "Admin"
        };

        // Act
        var result = await _userService.UpdateUserAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated@test.com", result.Email);
        Assert.Equal(UserRole.Admin, result.Role);
    }

    [Fact]
    public async Task UpdateUserAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            Email = "updated@test.com"
        };

        // Act
        var result = await _userService.UpdateUserAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidId_ShouldReturnTrue()
    {
        // Act
        var result = await _userService.DeleteUserAsync(1);

        // Assert
        Assert.True(result);
        
        // Verify user is deleted
        var deletedUser = await _userService.GetUserByIdAsync(1);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DeleteUserAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _userService.DeleteUserAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidRequest_ShouldReturnTrue()
    {
        // Arrange
        _authServiceMock.Setup(x => x.VerifyPassword("currentpassword", "hashedpassword"))
            .Returns(true);
        _authServiceMock.Setup(x => x.HashPassword("newpassword"))
            .Returns("newhashedpassword");

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "currentpassword",
            NewPassword = "newpassword"
        };

        // Act
        var result = await _userService.ChangePasswordAsync(1, request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ShouldReturnFalse()
    {
        // Arrange
        _authServiceMock.Setup(x => x.VerifyPassword("wrongpassword", "hashedpassword"))
            .Returns(false);

        var request = new ChangePasswordRequest
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword"
        };

        // Act
        var result = await _userService.ChangePasswordAsync(1, request);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}