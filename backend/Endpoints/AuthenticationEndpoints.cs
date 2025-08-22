using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.DTOs;
using ClinicAppointmentSystem.Services;

namespace ClinicAppointmentSystem.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        auth.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and get access token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);

        auth.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Logout and revoke refresh token")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IAuthenticationService authService)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return Results.BadRequest("Email and password are required");
        }

        var result = await authService.LoginAsync(request);
        
        if (result == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthenticationService authService)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Results.BadRequest("Refresh token is required");
        }

        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        
        if (result == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IAuthenticationService authService)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            await authService.RevokeTokenAsync(request.RefreshToken);
        }

        return Results.Ok(new { message = "Logged out successfully" });
    }
}