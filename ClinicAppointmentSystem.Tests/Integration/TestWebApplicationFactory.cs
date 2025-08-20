using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Endpoints;

namespace ClinicAppointmentSystem.Tests.Integration;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Reduce logging noise during tests
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            
            // Register mock background job service for testing
            services.AddScoped<ClinicAppointmentSystem.Services.IBackgroundJobService, MockBackgroundJobService>();

            // Build the service provider and seed data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Ensure the database is created
                context.Database.EnsureCreated();
                
                // Seed test data
                SeedTestData(context);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestWebApplicationFactory<TProgram>>>();
                logger.LogError(ex, "An error occurred seeding the database with test data.");
            }
        });
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed test clinic
        var clinic = new ClinicAppointmentSystem.Models.Clinic
        {
            Id = 1,
            Name = "Test Clinic",
            Address = "123 Test Street, Test City"
        };

        // Seed test doctor
        var doctor = new ClinicAppointmentSystem.Models.Doctor
        {
            Id = 1,
            ClinicId = 1,
            Name = "Dr. Test Doctor",
            Specialization = "General Practice",
            Email = "doctor@testclinic.com"
        };

        // Seed test admin user
        var adminUser = new ClinicAppointmentSystem.Models.User
        {
            Id = 1,
            ClinicId = 1,
            Email = "admin@testclinic.com",
            PasswordHash = "$2a$11$OTvljfKFp1GtdGihr7/peu8ynfVnlq2xu4IXOZq8pOFQ.Wyucq4t.", // "password123"
            Role = ClinicAppointmentSystem.Models.UserRole.Admin
        };

        // Seed test doctor user
        var doctorUser = new ClinicAppointmentSystem.Models.User
        {
            Id = 2,
            ClinicId = 1,
            DoctorId = 1,
            Email = "doctor@testclinic.com",
            PasswordHash = "$2a$11$OTvljfKFp1GtdGihr7/peu8ynfVnlq2xu4IXOZq8pOFQ.Wyucq4t.", // "password123"
            Role = ClinicAppointmentSystem.Models.UserRole.Doctor
        };

        // Seed test patients
        var patient1 = new ClinicAppointmentSystem.Models.Patient
        {
            Id = 1,
            ClinicId = 1,
            Name = "John Doe",
            Phone = "123-456-7890",
            Email = "john.doe@email.com",
            Notes = "Regular patient"
        };

        var patient2 = new ClinicAppointmentSystem.Models.Patient
        {
            Id = 2,
            ClinicId = 1,
            Name = "Jane Smith",
            Phone = "098-765-4321",
            Email = "jane.smith@email.com",
            Notes = "New patient"
        };

        // Seed test appointment
        var appointment = new ClinicAppointmentSystem.Models.Appointment
        {
            Id = 1,
            DoctorId = 1,
            PatientId = 1,
            DateTime = DateTime.UtcNow.AddDays(1),
            Status = ClinicAppointmentSystem.Models.AppointmentStatus.Scheduled,
            Notes = "Regular checkup"
        };

        context.Clinics.Add(clinic);
        context.Doctors.Add(doctor);
        context.Users.AddRange(adminUser, doctorUser);
        context.Patients.AddRange(patient1, patient2);
        context.Appointments.Add(appointment);

        context.SaveChanges();
    }
}