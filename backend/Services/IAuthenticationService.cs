using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public interface IAuthenticationService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}