using System.Security.Claims;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public static UserRole GetUserRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Doctor;
    }

    public static int GetClinicId(this ClaimsPrincipal principal)
    {
        var clinicIdClaim = principal.FindFirst("ClinicId")?.Value;
        return int.TryParse(clinicIdClaim, out var clinicId) ? clinicId : 0;
    }

    public static int? GetDoctorId(this ClaimsPrincipal principal)
    {
        var doctorIdClaim = principal.FindFirst("DoctorId")?.Value;
        return int.TryParse(doctorIdClaim, out var doctorId) ? doctorId : null;
    }

    public static bool IsDoctor(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(UserRole.Doctor.ToString());
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(UserRole.Admin.ToString());
    }

    public static bool CanAccessDoctor(this ClaimsPrincipal principal, int doctorId)
    {
        if (principal.IsAdmin())
            return true;

        if (principal.IsDoctor())
        {
            var userDoctorId = principal.GetDoctorId();
            return userDoctorId == doctorId;
        }

        return false;
    }

    public static bool CanAccessClinic(this ClaimsPrincipal principal, int clinicId)
    {
        var userClinicId = principal.GetClinicId();
        return userClinicId == clinicId;
    }
}