using Microsoft.AspNetCore.Authorization;
using ClinicAppointmentSystem.Models;

namespace ClinicAppointmentSystem.Authorization;

public class RoleAuthorizationAttribute : AuthorizeAttribute
{
    public RoleAuthorizationAttribute(params UserRole[] roles)
    {
        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}

public static class AuthorizationPolicies
{
    public const string DoctorOnly = "DoctorOnly";
    public const string AdminOnly = "AdminOnly";
    public const string DoctorOrAdmin = "DoctorOrAdmin";
}