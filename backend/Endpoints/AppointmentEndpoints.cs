using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Authorization;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Extensions;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var appointments = endpoints.MapGroup("/api/appointments")
            .WithTags("Appointment Management")
            .WithOpenApi()
            .RequireAuthorization();

        // Search appointments with filtering
        appointments.MapPost("/search", SearchAppointmentsAsync)
            .WithName("SearchAppointments")
            .WithSummary("Search appointments with filtering")
            .Produces<PagedResult<AppointmentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get appointment by ID
        appointments.MapGet("/{id:int}", GetAppointmentByIdAsync)
            .WithName("GetAppointmentById")
            .WithSummary("Get appointment by ID")
            .Produces<AppointmentDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Check appointment conflict
        appointments.MapPost("/check-conflict", CheckAppointmentConflictAsync)
            .WithName("CheckAppointmentConflict")
            .WithSummary("Check for appointment conflicts and get suggested times")
            .Produces<AppointmentConflictResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create new appointment
        appointments.MapPost("/", CreateAppointmentAsync)
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment")
            .Produces<AppointmentDetailsResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update appointment
        appointments.MapPut("/{id:int}", UpdateAppointmentAsync)
            .WithName("UpdateAppointment")
            .WithSummary("Update appointment information")
            .Produces<AppointmentDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete appointment
        appointments.MapDelete("/{id:int}", DeleteAppointmentAsync)
            .WithName("DeleteAppointment")
            .WithSummary("Delete an appointment")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get appointments by doctor (for calendar view)
        appointments.MapGet("/doctor/{doctorId:int}", GetAppointmentsByDoctorAsync)
            .WithName("GetAppointmentsByDoctor")
            .WithSummary("Get appointments for a specific doctor")
            .Produces<IEnumerable<AppointmentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get appointments by patient
        appointments.MapGet("/patient/{patientId:int}", GetAppointmentsByPatientAsync)
            .WithName("GetAppointmentsByPatient")
            .WithSummary("Get appointments for a specific patient")
            .Produces<IEnumerable<AppointmentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Admin-only endpoints
        var adminAppointments = appointments.MapGroup("/admin")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        // Get all appointments for clinic (Admin only)
        adminAppointments.MapPost("/search", SearchAllAppointmentsAsync)
            .WithName("SearchAllAppointments")
            .WithSummary("Search all appointments in clinic (Admin only)")
            .Produces<PagedResult<AppointmentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> SearchAppointmentsAsync(
        [FromBody] AppointmentSearchRequest request,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
        {
            return Results.BadRequest("Invalid page or page size parameters");
        }

        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();
        
        // For doctors, restrict to their own appointments
        int? restrictToDoctorId = null;
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null)
            {
                return Results.Forbid();
            }
            restrictToDoctorId = user.DoctorId.Value;
        }

        var (appointments, totalCount) = await appointmentService.SearchAppointmentsAsync(request, restrictToDoctorId);

        var result = new PagedResult<AppointmentResponse>
        {
            Items = appointments,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAppointmentByIdAsync(
        int id,
        [FromServices] IAppointmentService appointmentService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();

        // Check access permissions
        if (!await appointmentService.ValidateAppointmentAccessAsync(id, userId, userRole))
        {
            return Results.NotFound();
        }

        var appointment = await appointmentService.GetAppointmentDetailsAsync(id);
        if (appointment == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(appointment);
    }

    private static async Task<IResult> CheckAppointmentConflictAsync(
        [FromBody] CheckConflictRequest request,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();
        
        // Determine which doctor to check conflicts for
        int doctorId;
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null)
            {
                return Results.Forbid();
            }
            doctorId = user.DoctorId.Value;
        }
        else if (userRole == UserRole.Admin && request.DoctorId.HasValue)
        {
            doctorId = request.DoctorId.Value;
        }
        else
        {
            return Results.BadRequest("Doctor ID is required for admin users");
        }

        var conflictResponse = await appointmentService.CheckAppointmentConflictAsync(
            doctorId, request.DateTime, request.ExcludeAppointmentId);

        return Results.Ok(conflictResponse);
    }

    private static async Task<IResult> CreateAppointmentAsync(
        [FromBody] CreateAppointmentRequest request,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        if (request.PatientId <= 0)
        {
            return Results.BadRequest("Valid patient ID is required");
        }

        if (request.DateTime <= DateTime.UtcNow)
        {
            return Results.BadRequest("Appointment date must be in the future");
        }

        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();
        
        // Determine which doctor the appointment is for
        int doctorId;
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null)
            {
                return Results.Forbid();
            }
            doctorId = user.DoctorId.Value;
        }
        else
        {
            return Results.BadRequest("Only doctors can create appointments for themselves");
        }

        var appointment = await appointmentService.CreateAppointmentAsync(doctorId, request);
        if (appointment == null)
        {
            // Check if it was a conflict issue
            var conflictCheck = await appointmentService.CheckAppointmentConflictAsync(doctorId, request.DateTime);
            if (conflictCheck.HasConflict)
            {
                return Results.Conflict(new { 
                    message = "Appointment conflict detected", 
                    conflicts = conflictCheck.ConflictingAppointments,
                    suggestedTimes = conflictCheck.SuggestedTimes 
                });
            }
            return Results.BadRequest("Failed to create appointment");
        }

        var appointmentDetails = await appointmentService.GetAppointmentDetailsAsync(appointment.Id);
        return Results.Created($"/api/appointments/{appointment.Id}", appointmentDetails);
    }

    private static async Task<IResult> UpdateAppointmentAsync(
        int id,
        [FromBody] UpdateAppointmentRequest request,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();

        // Check access permissions first
        if (!await appointmentService.ValidateAppointmentAccessAsync(id, userId, userRole))
        {
            return Results.NotFound();
        }

        // For doctors, restrict updates to their own appointments
        int? restrictToDoctorId = null;
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null)
            {
                return Results.Forbid();
            }
            restrictToDoctorId = user.DoctorId.Value;
        }

        var appointment = await appointmentService.UpdateAppointmentAsync(id, request, restrictToDoctorId);
        if (appointment == null)
        {
            // Check if it was a conflict issue for datetime changes
            if (request.DateTime.HasValue)
            {
                var existingAppointment = await appointmentService.GetAppointmentByIdAsync(id);
                if (existingAppointment != null)
                {
                    var conflictCheck = await appointmentService.CheckAppointmentConflictAsync(
                        existingAppointment.DoctorId, request.DateTime.Value, id);
                    if (conflictCheck.HasConflict)
                    {
                        return Results.Conflict(new { 
                            message = "Appointment conflict detected", 
                            conflicts = conflictCheck.ConflictingAppointments,
                            suggestedTimes = conflictCheck.SuggestedTimes 
                        });
                    }
                }
            }
            return Results.BadRequest("Failed to update appointment");
        }

        var appointmentDetails = await appointmentService.GetAppointmentDetailsAsync(appointment.Id);
        return Results.Ok(appointmentDetails);
    }

    private static async Task<IResult> DeleteAppointmentAsync(
        int id,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();

        // Check access permissions
        if (!await appointmentService.ValidateAppointmentAccessAsync(id, userId, userRole))
        {
            return Results.NotFound();
        }

        // For doctors, restrict deletions to their own appointments
        int? restrictToDoctorId = null;
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null)
            {
                return Results.Forbid();
            }
            restrictToDoctorId = user.DoctorId.Value;
        }

        var success = await appointmentService.DeleteAppointmentAsync(id, restrictToDoctorId);
        if (!success)
        {
            return Results.BadRequest("Failed to delete appointment");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> GetAppointmentsByDoctorAsync(
        int doctorId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IUserService userService,
        HttpContext context)
    {
        var userId = context.User.GetUserId();
        var userRole = context.User.GetUserRole();

        // Doctors can only see their own appointments
        if (userRole == UserRole.Doctor)
        {
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.DoctorId == null || user.DoctorId.Value != doctorId)
            {
                return Results.Forbid();
            }
        }

        var appointments = await appointmentService.GetAppointmentsByDoctorIdAsync(doctorId, startDate, endDate);
        
        var appointmentResponses = appointments.Select(a => new AppointmentResponse
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.Name ?? "Unknown",
            PatientId = a.PatientId,
            PatientName = a.Patient?.Name ?? "Unknown",
            PatientPhone = a.Patient?.Phone,
            PatientEmail = a.Patient?.Email,
            DateTime = a.DateTime,
            Status = a.Status,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        });

        return Results.Ok(appointmentResponses);
    }

    private static async Task<IResult> GetAppointmentsByPatientAsync(
        int patientId,
        [FromServices] IAppointmentService appointmentService,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        
        // Check if patient exists in user's clinic
        if (!await patientService.PatientExistsInClinicAsync(patientId, clinicId))
        {
            return Results.NotFound();
        }

        var appointments = await appointmentService.GetAppointmentsByPatientIdAsync(patientId);
        
        var appointmentResponses = appointments.Select(a => new AppointmentResponse
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.Name ?? "Unknown",
            PatientId = a.PatientId,
            PatientName = a.Patient?.Name ?? "Unknown",
            PatientPhone = a.Patient?.Phone,
            PatientEmail = a.Patient?.Email,
            DateTime = a.DateTime,
            Status = a.Status,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        });

        return Results.Ok(appointmentResponses);
    }

    private static async Task<IResult> SearchAllAppointmentsAsync(
        [FromBody] AppointmentSearchRequest request,
        [FromServices] IAppointmentService appointmentService,
        HttpContext context)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
        {
            return Results.BadRequest("Invalid page or page size parameters");
        }

        // Admin can search all appointments without restriction
        var (appointments, totalCount) = await appointmentService.SearchAppointmentsAsync(request, null);

        var result = new PagedResult<AppointmentResponse>
        {
            Items = appointments,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Results.Ok(result);
    }
}

// Helper DTO for conflict checking
public class CheckConflictRequest
{
    public int? DoctorId { get; set; }
    public DateTime DateTime { get; set; }
    public int? ExcludeAppointmentId { get; set; }
}