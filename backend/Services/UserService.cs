using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Repositories;

namespace ClinicAppointmentSystem.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IAuthenticationService authService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetUsersByClinicIdAsync(int clinicId)
    {
        return await _userRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<User?> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                _logger.LogWarning("Attempt to create user with existing email: {Email}", request.Email);
                return null;
            }

            // Validate role
            if (!Enum.TryParse<UserRole>(request.Role, out var userRole))
            {
                _logger.LogWarning("Invalid role specified: {Role}", request.Role);
                return null;
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password),
                Role = userRole,
                ClinicId = request.ClinicId,
                DoctorId = request.DoctorId,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateAsync(user);
            _logger.LogInformation("User created successfully: {Email}", createdUser.Email);
            
            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", request.Email);
            return null;
        }
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for update: {UserId}", id);
                return null;
            }

            // Update email if provided
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await _userRepository.EmailExistsAsync(request.Email, id))
                {
                    _logger.LogWarning("Email already exists: {Email}", request.Email);
                    return null;
                }
                user.Email = request.Email;
            }

            // Update role if provided
            if (!string.IsNullOrEmpty(request.Role))
            {
                if (Enum.TryParse<UserRole>(request.Role, out var userRole))
                {
                    user.Role = userRole;
                }
            }

            // Update clinic ID if provided
            if (request.ClinicId.HasValue)
            {
                user.ClinicId = request.ClinicId.Value;
            }

            // Update doctor ID if provided
            if (request.DoctorId.HasValue)
            {
                user.DoctorId = request.DoctorId.Value;
            }

            var updatedUser = await _userRepository.UpdateAsync(user);
            _logger.LogInformation("User updated successfully: {UserId}", id);
            
            return updatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var result = await _userRepository.DeleteAsync(id);
            if (result)
            {
                _logger.LogInformation("User deleted successfully: {UserId}", id);
            }
            else
            {
                _logger.LogWarning("User not found for deletion: {UserId}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for password change: {UserId}", userId);
                return false;
            }

            // Verify current password
            if (!_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Invalid current password for user: {UserId}", userId);
                return false;
            }

            // Update password
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }
}