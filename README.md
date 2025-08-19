# Clinic Appointment Management System

A modern web application for managing clinic appointments with automated reminders.

## Project Structure

```
├── backend/                 # .NET 8 Web API
│   ├── Controllers/         # API Controllers
│   ├── Services/           # Business Logic Services
│   ├── Repositories/       # Data Access Layer
│   ├── Models/             # Entity Models
│   ├── DTOs/               # Data Transfer Objects
│   └── Data/               # Entity Framework DbContext
├── frontend/               # React TypeScript with Vite
│   ├── src/
│   │   ├── components/     # React Components
│   │   └── lib/            # Utility functions
│   └── components.json     # shadcn/ui configuration
└── .kiro/                  # Kiro specifications
    └── specs/
        └── clinic-appointment-system/
```

## Technology Stack

### Backend
- .NET 8 Web API
- Entity Framework Core with PostgreSQL
- JWT Authentication
- Hangfire for background jobs
- FluentValidation for input validation

### Frontend
- React 18 with TypeScript
- Vite for build tooling
- TailwindCSS for styling
- shadcn/ui for UI components

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- PostgreSQL 15+

## Getting Started

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Update the connection string in `appsettings.Development.json`

4. Run the application:
   ```bash
   dotnet run
   ```

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

## Configuration

### Database
Update the connection string in `backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=clinic_appointment_system;Username=postgres;Password=your_password"
  }
}
```

### External Services
Configure API keys in `appsettings.json`:
- SendGrid API key for email notifications
- WhatsApp Cloud API credentials for messaging

## Development

This project follows the specifications defined in `.kiro/specs/clinic-appointment-system/`. 

To continue development:
1. Review the requirements in `requirements.md`
2. Check the design document in `design.md`
3. Follow the implementation tasks in `tasks.md`

## Next Steps

The project structure is now set up. The next tasks involve:
1. Creating database models and Entity Framework setup
2. Implementing authentication and authorization
3. Building the appointment management API
4. Creating the React frontend components

Refer to the task list in `.kiro/specs/clinic-appointment-system/tasks.md` for detailed implementation steps.