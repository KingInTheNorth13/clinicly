using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Clinic)
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return null;
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (in a real app, you'd store this in database)
            // For now, we'll use a simple in-memory approach
            
            _logger.LogInformation("Successful login for user: {Email}", user.Email);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ClinicId = user.ClinicId,
                    DoctorId = user.DoctorId
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return null;
        }
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        // In a production app, you would validate the refresh token against database
        // For this MVP, we'll implement a basic version
        try
        {
            // This is a simplified implementation
            // In production, you'd store refresh tokens in database with expiration
            _logger.LogInformation("Refresh token request received");
            
            // For now, return null to indicate refresh token is invalid
            // This will be enhanced when we add proper refresh token storage
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            // In production, you'd remove the refresh token from database
            _logger.LogInformation("Token revocation requested");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return false;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("ClinicId", user.ClinicId.ToString()),
            new Claim("DoctorId", user.DoctorId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private int GetTokenExpirationMinutes()
    {
        return _configuration.GetValue<int>("JwtSettings:ExpirationMinutes", 60);
    }
}