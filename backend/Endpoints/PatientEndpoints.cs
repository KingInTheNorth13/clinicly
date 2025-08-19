using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Authorization;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Extensions;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var patients = endpoints.MapGroup("/api/patients")
            .WithTags("Patient Management")
            .WithOpenApi()
            .RequireAuthorization();

        // Get all patients for current user's clinic
        patients.MapGet("/", GetPatientsAsync)
            .WithName("GetPatients")
            .WithSummary("Get all patients in the current user's clinic")
            .Produces<IEnumerable<PatientResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        // Search patients with filtering
        patients.MapPost("/search", SearchPatientsAsync)
            .WithName("SearchPatients")
            .WithSummary("Search patients with filtering by name, phone, or email")
            .Produces<PagedResult<PatientResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get patient by ID
        patients.MapGet("/{id:int}", GetPatientByIdAsync)
            .WithName("GetPatientById")
            .WithSummary("Get patient by ID")
            .Produces<PatientResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get patient with appointments
        patients.MapGet("/{id:int}/appointments", GetPatientWithAppointmentsAsync)
            .WithName("GetPatientWithAppointments")
            .WithSummary("Get patient with their appointment history")
            .Produces<PatientWithAppointmentsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create new patient
        patients.MapPost("/", CreatePatientAsync)
            .WithName("CreatePatient")
            .WithSummary("Create a new patient")
            .Produces<PatientResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update patient
        patients.MapPut("/{id:int}", UpdatePatientAsync)
            .WithName("UpdatePatient")
            .WithSummary("Update patient information")
            .Produces<PatientResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete patient (Admin only)
        patients.MapDelete("/{id:int}", DeletePatientAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeletePatient")
            .WithSummary("Delete a patient (Admin only)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetPatientsAsync(
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        var patients = await patientService.GetPatientsByClinicIdAsync(clinicId);

        var patientResponses = patients.Select(p => new PatientResponse
        {
            Id = p.Id,
            ClinicId = p.ClinicId,
            Name = p.Name,
            Phone = p.Phone,
            Email = p.Email,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt,
            AppointmentCount = p.Appointments?.Count ?? 0
        });

        return Results.Ok(patientResponses);
    }

    private static async Task<IResult> SearchPatientsAsync(
        [FromBody] PatientSearchRequest request,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        if (request.Page < 1 || request.PageSize < 1 || request.PageSize > 100)
        {
            return Results.BadRequest("Invalid page or page size parameters");
        }

        var clinicId = context.User.GetClinicId();
        var (patients, totalCount) = await patientService.SearchPatientsAsync(clinicId, request);

        var patientResponses = patients.Select(p => new PatientResponse
        {
            Id = p.Id,
            ClinicId = p.ClinicId,
            Name = p.Name,
            Phone = p.Phone,
            Email = p.Email,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt,
            AppointmentCount = p.Appointments?.Count ?? 0
        });

        var result = new PagedResult<PatientResponse>
        {
            Items = patientResponses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Results.Ok(result);
    }

    private static async Task<IResult> GetPatientByIdAsync(
        int id,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        
        // Check if patient exists in user's clinic
        if (!await patientService.PatientExistsInClinicAsync(id, clinicId))
        {
            return Results.NotFound();
        }

        var patient = await patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return Results.NotFound();
        }

        var patientResponse = new PatientResponse
        {
            Id = patient.Id,
            ClinicId = patient.ClinicId,
            Name = patient.Name,
            Phone = patient.Phone,
            Email = patient.Email,
            Notes = patient.Notes,
            CreatedAt = patient.CreatedAt,
            AppointmentCount = patient.Appointments?.Count ?? 0
        };

        return Results.Ok(patientResponse);
    }

    private static async Task<IResult> GetPatientWithAppointmentsAsync(
        int id,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        
        // Check if patient exists in user's clinic
        if (!await patientService.PatientExistsInClinicAsync(id, clinicId))
        {
            return Results.NotFound();
        }

        var patient = await patientService.GetPatientWithAppointmentsAsync(id);
        if (patient == null)
        {
            return Results.NotFound();
        }

        var appointmentSummaries = patient.Appointments?.Select(a => new AppointmentSummary
        {
            Id = a.Id,
            DateTime = a.DateTime,
            Status = a.Status.ToString(),
            DoctorName = a.Doctor?.Name ?? "Unknown",
            Notes = a.Notes
        }).OrderByDescending(a => a.DateTime) ?? Enumerable.Empty<AppointmentSummary>();

        var patientResponse = new PatientWithAppointmentsResponse
        {
            Id = patient.Id,
            ClinicId = patient.ClinicId,
            Name = patient.Name,
            Phone = patient.Phone,
            Email = patient.Email,
            Notes = patient.Notes,
            CreatedAt = patient.CreatedAt,
            AppointmentCount = patient.Appointments?.Count ?? 0,
            Appointments = appointmentSummaries
        };

        return Results.Ok(patientResponse);
    }

    private static async Task<IResult> CreatePatientAsync(
        [FromBody] CreatePatientRequest request,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Patient name is required");
        }

        var clinicId = context.User.GetClinicId();
        var patient = await patientService.CreatePatientAsync(clinicId, request);

        if (patient == null)
        {
            return Results.BadRequest("Failed to create patient");
        }

        var patientResponse = new PatientResponse
        {
            Id = patient.Id,
            ClinicId = patient.ClinicId,
            Name = patient.Name,
            Phone = patient.Phone,
            Email = patient.Email,
            Notes = patient.Notes,
            CreatedAt = patient.CreatedAt,
            AppointmentCount = 0
        };

        return Results.Created($"/api/patients/{patient.Id}", patientResponse);
    }

    private static async Task<IResult> UpdatePatientAsync(
        int id,
        [FromBody] UpdatePatientRequest request,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        
        // Check if patient exists in user's clinic
        if (!await patientService.PatientExistsInClinicAsync(id, clinicId))
        {
            return Results.NotFound();
        }

        var patient = await patientService.UpdatePatientAsync(id, request);
        if (patient == null)
        {
            return Results.BadRequest("Failed to update patient");
        }

        var patientResponse = new PatientResponse
        {
            Id = patient.Id,
            ClinicId = patient.ClinicId,
            Name = patient.Name,
            Phone = patient.Phone,
            Email = patient.Email,
            Notes = patient.Notes,
            CreatedAt = patient.CreatedAt,
            AppointmentCount = patient.Appointments?.Count ?? 0
        };

        return Results.Ok(patientResponse);
    }

    private static async Task<IResult> DeletePatientAsync(
        int id,
        [FromServices] IPatientService patientService,
        HttpContext context)
    {
        var clinicId = context.User.GetClinicId();
        
        // Check if patient exists in user's clinic
        if (!await patientService.PatientExistsInClinicAsync(id, clinicId))
        {
            return Results.NotFound();
        }

        var success = await patientService.DeletePatientAsync(id);
        if (!success)
        {
            return Results.BadRequest("Failed to delete patient");
        }

        return Results.NoContent();
    }
}