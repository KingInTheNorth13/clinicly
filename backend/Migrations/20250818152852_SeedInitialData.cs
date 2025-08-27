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
                VALUES (1, NULL, 'admin@democlinic.com', '$2a$11$N9qo8uLOickgx2ZMRZoMye.bJe68LJlz/RZlbxyS3L16toGdsOlW6', 'Admin', CURRENT_TIMESTAMP)
                ON CONFLICT (""Email"") DO NOTHING;
            ");

            // Insert doctor user account
            // Password: "doctor123" - Note: In production, use proper password hashing
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""ClinicId"", ""DoctorId"", ""Email"", ""PasswordHash"", ""Role"", ""CreatedAt"") 
                VALUES (1, 1, 'dr.smith@democlinic.com', '$2a$11$6BNUOWmnLGp4vAHTABbiYu.hCxUxFkRIgtnMdOWYg3dMfW9lWtHiq', 'Doctor', CURRENT_TIMESTAMP)
                ON CONFLICT (""Email"") DO NOTHING;
            ");

            // Insert sample patients
            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'Jane Doe', '+1-555-0123', 'jane.doe@email.com', 'Regular patient for annual checkups', CURRENT_TIMESTAMP);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'John Smith', '+1-555-0124', 'john.smith@email.com', 'New patient consultation', CURRENT_TIMESTAMP);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'Mary Johnson', '+1-555-0125', 'mary.johnson@email.com', 'Follow-up appointment needed', CURRENT_TIMESTAMP);
            ");

            // Insert sample appointments
            migrationBuilder.Sql(@"
                INSERT INTO ""Appointments"" (""DoctorId"", ""PatientId"", ""DateTime"", ""Status"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 1, '2025-08-27 10:00:00', 0, 'Annual checkup appointment', CURRENT_TIMESTAMP);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Appointments"" (""DoctorId"", ""PatientId"", ""DateTime"", ""Status"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 2, '2025-08-27 10:30:00', 0, 'New patient consultation', CURRENT_TIMESTAMP);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Appointments"" (""DoctorId"", ""PatientId"", ""DateTime"", ""Status"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 3, '2025-08-28 14:00:00', 0, 'Follow-up appointment', CURRENT_TIMESTAMP);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Appointments"" (""DoctorId"", ""PatientId"", ""DateTime"", ""Status"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 1, '2025-08-29 09:00:00', 1, 'Completed consultation', CURRENT_TIMESTAMP);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data in reverse order
            migrationBuilder.Sql(@"DELETE FROM ""Appointments"" WHERE ""DoctorId"" = 1;");
            migrationBuilder.Sql(@"DELETE FROM ""Patients"" WHERE ""Email"" IN ('jane.doe@email.com', 'john.smith@email.com', 'mary.johnson@email.com');");
            migrationBuilder.Sql(@"DELETE FROM ""Users"" WHERE ""Email"" IN ('admin@democlinic.com', 'dr.smith@democlinic.com');");
            migrationBuilder.Sql(@"DELETE FROM ""Doctors"" WHERE ""Email"" = 'dr.smith@democlinic.com';");
            migrationBuilder.Sql(@"DELETE FROM ""Clinics"" WHERE ""Name"" = 'Demo Clinic';");
        }
    }
}
