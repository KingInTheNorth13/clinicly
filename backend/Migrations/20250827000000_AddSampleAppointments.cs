using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicAppointmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSampleAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert additional sample patients
            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'John Smith', '+1-555-0124', 'john.smith@email.com', 'New patient consultation', CURRENT_TIMESTAMP)
                ON CONFLICT (""Email"") DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO ""Patients"" (""ClinicId"", ""Name"", ""Phone"", ""Email"", ""Notes"", ""CreatedAt"") 
                VALUES (1, 'Mary Johnson', '+1-555-0125', 'mary.johnson@email.com', 'Follow-up appointment needed', CURRENT_TIMESTAMP)
                ON CONFLICT (""Email"") DO NOTHING;
            ");

            // Delete any existing appointments to avoid conflicts
            migrationBuilder.Sql(@"DELETE FROM ""Appointments"" WHERE ""DoctorId"" = 1;");

            // Insert sample appointments with proper patient associations
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
            // Remove sample appointments and additional patients
            migrationBuilder.Sql(@"DELETE FROM ""Appointments"" WHERE ""DoctorId"" = 1;");
            migrationBuilder.Sql(@"DELETE FROM ""Patients"" WHERE ""Email"" IN ('john.smith@email.com', 'mary.johnson@email.com');");
        }
    }
}