using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByClinicIdAsync(int clinicId);
    Task<User?> CreateUserAsync(CreateUserRequest request);
    Task<User?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}