# Database Setup Instructions

## Prerequisites

1. Install PostgreSQL 15+ on your system
2. Create a database named `clinic_appointment_system`
3. Update the connection string in `appsettings.json` with your PostgreSQL credentials

## Running Migrations

To apply the database migrations and create all tables with seed data:

```bash
# Navigate to the backend directory
cd backend

# Update the database with migrations
dotnet ef database update
```

## Migration Details

### InitialCreate Migration
- Creates all core tables: Clinics, Doctors, Patients, Users, Appointments
- Sets up foreign key relationships
- Creates unique constraints (doctor email, user email)
- Implements unique constraint for doctor_id + datetime to prevent double booking
- Creates performance indexes on frequently queried columns

### SeedInitialData Migration
- Inserts a demo clinic
- Creates an admin user (email: admin@democlinic.com, password: admin123)
- Creates a sample doctor (Dr. John Smith)
- Creates a doctor user account (email: dr.smith@democlinic.com, password: doctor123)
- Inserts a sample patient (Jane Doe)

## Database Schema

The database includes the following tables:

- **Clinics**: Clinic information
- **Doctors**: Doctor profiles linked to clinics
- **Patients**: Patient records linked to clinics
- **Users**: Authentication accounts for doctors and admins
- **Appointments**: Appointment scheduling with double-booking prevention

## Performance Indexes

The following indexes are created for optimal performance:

- `IX_Appointments_Doctor_DateTime`: Unique index preventing double booking
- `IX_Appointments_Patient`: Index for patient appointment lookups
- `IX_Doctors_Clinic`: Index for clinic-doctor relationships
- `IX_Patients_Clinic`: Index for clinic-patient relationships
- `IX_Doctors_Email`: Unique index for doctor email addresses
- `IX_Users_Email`: Unique index for user email addresses

## Connection String Format

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=clinic_appointment_system;Username=your_username;Password=your_password"
  }
}
```

## Troubleshooting

If you encounter issues:

1. Ensure PostgreSQL is running
2. Verify database exists and connection string is correct
3. Check that the user has proper permissions
4. Run `dotnet ef migrations list` to see migration status