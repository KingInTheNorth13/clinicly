# Implementation Plan

## Medical Design System Guidelines

All UI components should follow the established medical theme design system:

### Color Palette
- **Primary Backgrounds**: Dark slate (`#0f172a`, `#1e293b`, `#334155`)
- **Primary Accents**: Teal/Cyan gradients (`#14b8a6`, `#0891b2`, `#10b981`)
- **Text Colors**: Light slate (`#f8fafc`, `#e2e8f0`, `#94a3b8`)
- **Status Colors**: Red (`#f87171`) for errors, Amber for warnings, Teal for success

### Typography
- **Font Family**: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif
- **Font Weights**: 400 (normal), 500 (medium), 600 (semibold), 700 (bold), 800 (extrabold)
- **Responsive Sizing**: Use clamp() for scalable text sizes

### Visual Elements
- **Border Radius**: 16px for inputs, 28px for cards and modals
- **Shadows**: Multi-layered with teal accent glows for interactive elements
- **Backdrop Effects**: `backdrop-filter: blur(20px)` for glass morphism
- **Animations**: medicalFloat, medicalPulse, medicalSparkle for medical-themed motion

### Medical Icons
- **Primary**: Stethoscope, Heart, Shield, Calendar
- **Secondary**: Lock, Mail, Eye, HelpCircle, Loader2
- **Status**: CheckCircle, AlertCircle, XCircle

### Layout Principles
- **Full Viewport**: Use `position: fixed` with `top: 0, left: 0, right: 0, bottom: 0` for full-screen layouts
- **Centering**: Flexbox with `align-items: center` and `justify-content: center`
- **Spacing**: Consistent margins and padding using medical-appropriate spacing scale
- **Responsive**: Mobile-first approach with proper breakpoints

- [x] 1. Set up project structure and development environment
  - Create .NET 8 Web API project with proper folder structure (Controllers, Services, Repositories, Models, DTOs)
  - Create React TypeScript project with Vite and configure TailwindCSS
  - Configure PostgreSQL connection and Entity Framework Core
  - Set up basic project dependencies and NuGet packages (Entity Framework, JWT, Hangfire, FluentValidation)
  - Install and configure shadcn/ui components for React frontend
  - _Requirements: 9.1, 9.2, 9.3_

- [x] 2. Implement database models and Entity Framework setup
  - [x] 2.1 Create Entity Framework models and DbContext
    - Write entity classes for Clinic, Doctor, Patient, User, and Appointment
    - Configure DbContext with proper relationships and constraints
    - Implement unique constraint for doctor_id + datetime to prevent double booking
    - _Requirements: 3.2, 9.3_

  - [x] 2.2 Create and run database migrations
    - Generate initial migration with all tables and relationships
    - Create database indexes for performance optimization
    - Write seed data for initial clinic and admin user
    - _Requirements: 9.3_

- [x] 3. Implement authentication and authorization system
  - [x] 3.1 Create JWT authentication service
    - Implement JWT token generation and validation logic
    - Create password hashing utilities using BCrypt
    - Write authentication middleware for API endpoints
    - _Requirements: 1.1, 1.2, 1.4_

  - [x] 3.2 Implement user management and role-based access
    - Create User entity and authentication endpoints (login, refresh, logout)
    - Implement role-based authorization attributes for Doctor and Admin roles
    - Write unit tests for authentication service and middleware
    - _Requirements: 1.3, 5.4_

- [ ] 4. Build patient management system
  - [x] 4.1 Implement patient repository and service layer

    - Create IPatientRepository interface and implementation
    - Implement PatientService with CRUD operations and search functionality
    - Add patient-appointment relationship management
    - _Requirements: 6.1, 6.2, 6.4_

  - [x] 4.2 Create patient API endpoints





    - Implement Minimal API endpoints for patient management
    - Add search functionality with filtering by name and contact information
    - Implement patient-appointment linking logic
    - Write integration tests for patient endpoints
    - _Requirements: 6.3_

- [x] 5. Build core appointment management API




  - [x] 5.1 Implement appointment repository and service layer


    - Create IAppointmentRepository interface and implementation
    - Implement AppointmentService with business logic for CRUD operations
    - Add validation to prevent double booking conflicts
    - _Requirements: 3.1, 3.2, 3.3, 4.1, 4.3_

  - [x] 5.2 Create appointment API endpoints


    - Implement Minimal API endpoints for appointment CRUD operations
    - Add role-based filtering (doctors see only their appointments)
    - Implement appointment conflict detection and error handling
    - Write integration tests for appointment endpoints
    - _Requirements: 2.4, 4.2, 5.1, 5.3_

- [x] 6. Integrate email notification service





  - [x] 6.1 Implement SendGrid email service


    - Install SendGrid NuGet package and configure API key
    - Create IEmailService interface and SendGridEmailService implementation
    - Implement email templates for appointment reminders
    - Add error handling and delivery status tracking
    - Write unit tests for email service
    - _Requirements: 7.2, 7.4_

  - [x] 6.2 Create notification orchestration service


    - Implement INotificationService interface and NotificationService implementation
    - Add retry logic with exponential backoff for failed notifications
    - Implement notification logging and status tracking
    - Create extensible interface for future notification channels (WhatsApp, SMS)
    - Write unit tests for notification service
    - _Requirements: 8.4_

- [ ] 7. Implement Hangfire background job processing for reminders





  - [x] 7.1 Create reminder scheduling system


    - Create IReminderService interface and ReminderService implementation
    - Implement reminder job that schedules 24 hours before appointments
    - Add job scheduling when appointments are created or updated
    - Add job cancellation when appointments are cancelled or rescheduled
    - _Requirements: 7.1, 8.1, 8.2, 8.3_

  - [x] 7.2 Integrate reminder system with appointment management


    - Update appointment service to schedule/cancel reminder jobs
    - Implement background job processing for reminder delivery
    - Add job retry logic and error handling
    - Write integration tests for reminder scheduling system
    - _Requirements: 8.3, 8.4_

- [x] 8. Build React frontend foundation





  - [x] 8.1 Install required frontend dependencies


    - Install React Router, Axios, React Hook Form, Zod, FullCalendar
    - Install additional shadcn/ui components (Form, Dialog, DataTable, etc.)
    - Set up proper TypeScript types and interfaces
    - Configure environment variables for API base URL
    - _Requirements: 9.2_

  - [x] 8.2 Set up React project structure and routing


    - Set up React Router for navigation between pages
    - Create basic project structure with components, hooks, services, and types folders
    - Create basic layout components using shadcn/ui
    - Replace default App.tsx with proper application structure
    - _Requirements: 9.2_

  - [x] 8.3 Implement authentication context and API client


    - Create AuthContext for managing JWT tokens and user state
    - Implement Axios client with interceptors for API communication
    - Create authentication service for login, logout, and token refresh
    - Add protected route wrapper component with role-based access
    - _Requirements: 1.1, 1.2, 1.3_

- [x] 9. Build authentication UI components






  - [x] 9.1 Create login page with shadcn/ui components

    - Implement LoginPage component using shadcn/ui Card and Form components, make it modern and add some good animations.
    - Add form validation using React Hook Form and Zod
    - Implement login functionality with error handling and loading states
    - Add responsive design with TailwindCSS
    - _Requirements: 1.1, 1.4_



  - [x] 9.2 Create protected routing and navigation

    - Implement ProtectedRoute component with role-based access control
    - Create navigation header with user info and logout functionality
    - Add route guards for Doctor and Admin specific pages
    - _Requirements: 1.3, 5.4_

- [x] 10. Build modern interactive appointment management UI with medical theme





  - [x] 10.1 Create calendar view with FullCalendar integration and medical theming


    - Implement CalendarView component with FullCalendar using medical color scheme (teal/cyan gradients)
    - Apply consistent medical theming: dark slate backgrounds, teal accents, medical icons (Stethoscope, Heart, Calendar)
    - Integrate with appointment API to display appointments with status-based color coding
    - Add appointment click handlers to show details in shadcn/ui Dialog with medical-themed styling
    - Implement role-based filtering (doctors see only their appointments) with animated transitions
    - Use medical-themed animations (medicalFloat, medicalPulse) for interactive elements
    - _Requirements: 2.1, 2.2, 2.3, 2.4_



  - [x] 10.2 Create appointment form components with medical design system

    - Implement AppointmentForm using shadcn/ui Dialog and Form components with medical theming
    - Apply consistent styling: dark slate backgrounds, teal gradients, rounded corners (28px), medical icons
    - Add DatePicker and TimePicker with medical-themed styling and hover effects
    - Implement patient selection with search functionality using medical color palette
    - Add form validation with medical-themed error states and success animations
    - Include medical icons (Shield, Lock, Calendar) for form elements and buttons
    - Use backdrop blur effects and subtle shadows consistent with login page design

    - _Requirements: 3.1, 3.2, 3.4_

  - [x] 10.3 Build appointment list and management features with medical UI

    - Create AppointmentList using shadcn/ui DataTable with medical theming and sorting/filtering
    - Apply medical color scheme: dark backgrounds, teal accents, medical status indicators
    - Implement appointment status updates (completed, cancelled, no-show) with medical-themed badges
    - Add appointment details view with patient information using medical card design
    - Create appointment update and cancellation functionality with medical-themed modals
    - Include medical icons (Heart, Shield, Calendar) and smooth animations throughout
    - Use consistent typography (Inter font) and spacing from established design system
    - _Requirements: 4.1, 4.2, 4.3_

- [ ] 11. Build patient management UI with medical design system
  - [ ] 11.1 Create patient list and search interface with medical theming
    - Implement PatientList using shadcn/ui DataTable with medical color scheme and search functionality
    - Apply medical theming: dark slate backgrounds, teal gradients, medical icons (Heart, Shield)
    - Add Command palette for quick patient search with medical-themed styling and animations
    - Implement patient filtering and sorting capabilities with medical-themed controls
    - Create patient selection interface for appointment booking with medical card design
    - Use consistent medical animations (medicalFloat, medicalPulse) for interactive elements
    - Include medical status indicators and hover effects consistent with login page design
    - _Requirements: 6.4_

  - [ ] 11.2 Create patient form and profile components with medical UI
    - Implement PatientForm using shadcn/ui Form components with medical theming and validation
    - Apply medical design system: dark backgrounds, teal accents, rounded corners, backdrop blur
    - Create PatientProfile component showing patient details with medical card styling
    - Add patient creation, editing, and deletion functionality with medical-themed modals
    - Implement patient-appointment relationship display using medical timeline design
    - Include medical icons (Stethoscope, Heart, Calendar) and consistent typography (Inter font)
    - Use medical color palette for form states, success/error messages, and loading animations
    - _Requirements: 6.1, 6.2, 6.3_

- [ ] 12. Build admin dashboard features with medical executive theme
  - [ ] 12.1 Create admin dashboard with clinic-wide view and medical theming
    - Implement AdminDashboard with shadcn/ui Tabs using medical color scheme and executive styling
    - Apply premium medical theming: dark slate backgrounds, teal/cyan gradients, professional medical icons
    - Create clinic-wide appointment calendar with medical status indicators and doctor color coding
    - Add statistics cards with medical-themed metrics display, animated counters, and medical icons
    - Implement doctor selection filters with medical-themed dropdown and search functionality
    - Use medical animations (medicalPulse, medicalFloat) for dashboard elements and data visualization
    - Include executive-level medical branding with professional typography and spacing
    - _Requirements: 5.1, 5.2_

  - [ ] 12.2 Implement admin appointment management with medical authority theme
    - Create admin appointment management interface with medical-themed CRUD capabilities
    - Apply medical authority styling: enhanced permissions UI, medical badges, professional layouts
    - Add doctor assignment functionality with medical-themed selection interface
    - Implement bulk operations with medical-themed batch processing UI and confirmation dialogs
    - Create admin-specific conflict resolution tools with medical priority indicators
    - Include medical icons (Shield, Lock, Stethoscope) for admin functions and security features
    - Use consistent medical color palette with enhanced admin-level visual hierarchy
    - _Requirements: 5.3_

- [ ] 13. Add error handling and user feedback with medical UX design
  - [ ] 13.1 Implement global error handling with medical theming
    - Create global error boundary for React component errors with medical-themed error pages
    - Add toast notification system using shadcn/ui Toast with medical color scheme and icons
    - Implement API error handling with medical-themed user-friendly messages and recovery suggestions
    - Add loading states and skeleton components with medical animations (medicalPulse, spin)
    - Use medical color palette for error states: red for critical, amber for warnings, teal for success
    - Include medical icons (Heart, Shield, HelpCircle) for different notification types
    - Apply consistent medical styling: dark backgrounds, rounded corners, backdrop blur effects
    - _Requirements: 3.4, 4.2_

  - [ ] 13.2 Add form validation and user feedback with medical design system
    - Implement real-time form validation with medical-themed error messages and animations
    - Add success notifications with medical celebration animations and teal color scheme
    - Create confirmation dialogs for destructive actions using medical-themed modals with medical icons
    - Implement network error detection with medical-themed retry mechanisms and status indicators
    - Use medical typography (Inter font) and consistent spacing for all feedback elements
    - Include medical progress indicators and loading animations consistent with login page design
    - Apply medical hover effects and transitions for interactive feedback elements
    - _Requirements: 1.4, 3.4_

- [ ] 14. Configure deployment and environment setup
  - [ ] 14.1 Prepare backend for deployment
    - Configure appsettings.json for different environments (Development, Production)
    - Set up environment variable configuration for database connection and API keys
    - Add health check endpoints for monitoring
    - Configure CORS settings for frontend communication
    - _Requirements: 10.1, 10.3_

  - [ ] 14.2 Prepare frontend for deployment
    - Configure build settings and environment variables
    - Set up API base URL configuration for different environments
    - Optimize build output with proper code splitting
    - Create production-ready build configuration
    - _Requirements: 10.2, 10.3_

  - [ ] 14.3 Create environment configuration and documentation
    - Create sample .env files with placeholder values for all required API keys
    - Document SendGrid API setup and email template configuration
    - Create deployment guide with step-by-step instructions
    - Add WhatsApp Cloud API configuration documentation
    - _Requirements: 10.4_

- [ ] 15. Write comprehensive tests
  - [ ] 15.1 Create backend unit and integration tests
    - Write unit tests for service layer components (AppointmentService, PatientService, AuthenticationService)
    - Create integration tests for API endpoints using WebApplicationFactory
    - Add repository layer tests with in-memory database
    - Implement background job testing for Hangfire reminder system
    - _Requirements: 8.3, 8.4_

  - [ ] 15.2 Create frontend component and integration tests
    - Write unit tests for React components using Jest and React Testing Library
    - Create integration tests for authentication flow and API communication
    - Add tests for form validation and error handling
    - Implement accessibility tests using axe-core for WCAG compliance
    - _Requirements: 1.1, 2.1, 3.1_
#
# Future Enhancements (Optional)

- [ ] 16. WhatsApp Cloud API Integration (Future Phase)
  - [ ] 16.1 Implement WhatsApp Cloud API service
    - Create WhatsApp service for sending messages
    - Implement message templates for appointment reminders
    - Add WhatsApp-specific error handling and delivery tracking
    - _Requirements: 7.1, 7.3_

  - [ ] 16.2 Update notification orchestration for WhatsApp
    - Modify NotificationService to support WhatsApp as primary channel
    - Implement WhatsApp-to-email fallback logic
    - Add WhatsApp delivery status tracking and reporting
    - Update environment configuration for WhatsApp API keys
    - _Requirements: 7.1, 7.2_