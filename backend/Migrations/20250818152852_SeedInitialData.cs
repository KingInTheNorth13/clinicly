using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert initial clinic
            migrationBuilder.Sql(@"
                INSERT INTO ""Clinics"" (""Name"", ""Address"", ""CreatedAt"") 
                VALUES ('Demo Clinic', '123 Main Street, City, State 12345', CURRENT_TIMESTAMP);
            ");

            // Insert sample doctor
            migrationBuilder.Sql(@"
                INSERT INTO ""Doctors"" (""ClinicId"", ""Name"", ""Specialization"", ""Email"", ""CreatedAt"") 
                VALUES (1, 'Dr. John Smith', 'General Practice', 'dr.smith@democlinic.com', CURRENT_TIMESTAMP);
            ");

            // Insert initial admin user
            // Password: "admin123" - Note: In production, use proper password hashing
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""ClinicId"", ""DoctorId"", ""Email"", ""PasswordHash"", ""Role"", ""CreatedAt"") 
                VALUES (1, NULL, 'admin@democlinic.com', '$2a$11$rQZJKjKjKjKjKjKjKjKjKOeH8vKjKjKjKjKjKjKjKjKjKjKjKjKjK', 'Admin', CURRENT_TIMESTAMP);
            ");

            // Insert doctor user account
            // Password: "doctor123" - Note: In production, use proper password hashing
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""ClinicId"", ""DoctorId"", ""Email"", ""PasswordHash"", ""Role"", ""CreatedAt"") 
                VALUES (1, 1, 'dr.smith@democlinic.com', '$2a$11$sQZJKjKjKjKjKjKjKjKjKOeH8vKjKjKjKjKjKjKjKjKjKjKjKjKjL', 'Doctor', CURRENT_TIMESTAMP);
            ");

            // Insert sample patient
            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'Jane Doe', '+1-555-0123', 'jane.doe@email.com', 'Regular patient for annual checkups', CURRENT_TIMESTAMP);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data in reverse order
            migrationBuilder.Sql(@"DELETE FROM ""Patients"" WHERE ""Email"" = 'jane.doe@email.com';");
            migrationBuilder.Sql(@"DELETE FROM ""Users"" WHERE ""Email"" IN ('admin@democlinic.com', 'dr.smith@democlinic.com');");
            migrationBuilder.Sql(@"DELETE FROM ""Doctors"" WHERE ""Email"" = 'dr.smith@democlinic.com';");
            migrationBuilder.Sql(@"DELETE FROM ""Clinics"" WHERE ""Name"" = 'Demo Clinic';");
        }
    }
}
