using System.Security.Claims;
using Xunit;
using ClinicAppointmentSystem.Extensions;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Tests.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    private ClaimsPrincipal CreateTestPrincipal(UserRole role, int userId = 1, int clinicId = 1, int? doctorId = 1)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, role.ToString()),
            new("ClinicId", clinicId.ToString())
        };

        if (doctorId.HasValue)
        {
            claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public void GetUserId_ShouldReturnCorrectUserId()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, userId: 123);

        // Act
        var result = principal.GetUserId();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void GetEmail_ShouldReturnCorrectEmail()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor);

        // Act
        var result = principal.GetEmail();

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void GetUserRole_ShouldReturnCorrectRole()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Admin);

        // Act
        var result = principal.GetUserRole();

        // Assert
        Assert.Equal(UserRole.Admin, result);
    }

    [Fact]
    public void GetClinicId_ShouldReturnCorrectClinicId()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, clinicId: 456);

        // Act
        var result = principal.GetClinicId();

        // Assert
        Assert.Equal(456, result);
    }

    [Fact]
    public void GetDoctorId_ShouldReturnCorrectDoctorId()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, doctorId: 789);

        // Act
        var result = principal.GetDoctorId();

        // Assert
        Assert.Equal(789, result);
    }

    [Fact]
    public void GetDoctorId_WithNullDoctorId_ShouldReturnNull()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Admin, doctorId: null);

        // Act
        var result = principal.GetDoctorId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsDoctor_WithDoctorRole_ShouldReturnTrue()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor);

        // Act
        var result = principal.IsDoctor();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDoctor_WithAdminRole_ShouldReturnFalse()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Admin);

        // Act
        var result = principal.IsDoctor();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ShouldReturnTrue()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Admin);

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAdmin_WithDoctorRole_ShouldReturnFalse()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor);

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanAccessDoctor_AsAdmin_ShouldReturnTrue()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Admin);

        // Act
        var result = principal.CanAccessDoctor(123);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAccessDoctor_AsDoctorWithSameId_ShouldReturnTrue()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, doctorId: 123);

        // Act
        var result = principal.CanAccessDoctor(123);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAccessDoctor_AsDoctorWithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, doctorId: 123);

        // Act
        var result = principal.CanAccessDoctor(456);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanAccessClinic_WithSameClinicId_ShouldReturnTrue()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, clinicId: 123);

        // Act
        var result = principal.CanAccessClinic(123);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAccessClinic_WithDifferentClinicId_ShouldReturnFalse()
    {
        // Arrange
        var principal = CreateTestPrincipal(UserRole.Doctor, clinicId: 123);

        // Act
        var result = principal.CanAccessClinic(456);

        // Assert
        Assert.False(result);
    }
}