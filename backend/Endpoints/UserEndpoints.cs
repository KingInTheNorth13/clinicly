using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Authorization;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Extensions;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var users = endpoints.MapGroup("/api/users")
            .WithTags("User Management")
            .WithOpenApi()
            .RequireAuthorization();

        // Admin only endpoints
        users.MapPost("/", CreateUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateUser")
            .WithSummary("Create a new user (Admin only)")
            .Produces<UserInfo>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        users.MapGet("/clinic/{clinicId:int}", GetUsersByClinicAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetUsersByClinic")
            .WithSummary("Get all users in a clinic (Admin only)")
            .Produces<IEnumerable<UserInfo>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        users.MapPut("/{id:int}", UpdateUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateUser")
            .WithSummary("Update user information (Admin only)")
            .Produces<UserInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        users.MapDelete("/{id:int}", DeleteUserAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteUser")
            .WithSummary("Delete a user (Admin only)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        // User profile endpoints (accessible by the user themselves)
        users.MapGet("/profile", GetCurrentUserProfileAsync)
            .WithName("GetCurrentUserProfile")
            .WithSummary("Get current user's profile")
            .Produces<UserInfo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        users.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change current user's password")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return Results.BadRequest("Email and password are required");
        }

        // Ensure admin can only create users in their own clinic
        var currentUserClinicId = context.User.GetClinicId();
        if (request.ClinicId != currentUserClinicId)
        {
            return Results.Forbid();
        }

        var user = await userService.CreateUserAsync(request);
        
        if (user == null)
        {
            return Results.BadRequest("Failed to create user. Email may already exist.");
        }

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            ClinicId = user.ClinicId,
            DoctorId = user.DoctorId
        };

        return Results.Created($"/api/users/{user.Id}", userInfo);
    }

    private static async Task<IResult> GetUsersByClinicAsync(
        int clinicId,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        // Ensure admin can only access users in their own clinic
        var currentUserClinicId = context.User.GetClinicId();
        if (clinicId != currentUserClinicId)
        {
            return Results.Forbid();
        }

        var users = await userService.GetUsersByClinicIdAsync(clinicId);
        
        var userInfos = users.Select(u => new UserInfo
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role.ToString(),
            ClinicId = u.ClinicId,
            DoctorId = u.DoctorId
        });

        return Results.Ok(userInfos);
    }

    private static async Task<IResult> UpdateUserAsync(
        int id,
        [FromBody] UpdateUserRequest request,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return Results.NotFound();
        }

        // Ensure admin can only update users in their own clinic
        var currentUserClinicId = context.User.GetClinicId();
        if (user.ClinicId != currentUserClinicId)
        {
            return Results.Forbid();
        }

        var updatedUser = await userService.UpdateUserAsync(id, request);
        
        if (updatedUser == null)
        {
            return Results.BadRequest("Failed to update user");
        }

        var userInfo = new UserInfo
        {
            Id = updatedUser.Id,
            Email = updatedUser.Email,
            Role = updatedUser.Role.ToString(),
            ClinicId = updatedUser.ClinicId,
            DoctorId = updatedUser.DoctorId
        };

        return Results.Ok(userInfo);
    }

    private static async Task<IResult> DeleteUserAsync(
        int id,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return Results.NotFound();
        }

        // Ensure admin can only delete users in their own clinic
        var currentUserClinicId = context.User.GetClinicId();
        if (user.ClinicId != currentUserClinicId)
        {
            return Results.Forbid();
        }

        // Prevent admin from deleting themselves
        var currentUserId = context.User.GetUserId();
        if (id == currentUserId)
        {
            return Results.BadRequest("Cannot delete your own account");
        }

        var success = await userService.DeleteUserAsync(id);
        
        if (!success)
        {
            return Results.BadRequest("Failed to delete user");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUserProfileAsync(
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var user = await userService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return Results.NotFound();
        }

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            ClinicId = user.ClinicId,
            DoctorId = user.DoctorId
        };

        return Results.Ok(userInfo);
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
        {
            return Results.BadRequest("Current password and new password are required");
        }

        var userId = context.User.GetUserId();
        var success = await userService.ChangePasswordAsync(userId, request);
        
        if (!success)
        {
            return Results.BadRequest("Failed to change password. Current password may be incorrect.");
        }

        return Results.Ok(new { message = "Password changed successfully" });
    }
}