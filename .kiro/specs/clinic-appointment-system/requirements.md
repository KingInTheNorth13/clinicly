# Requirements Document

## Introduction

This document outlines the requirements for a clinic appointment management system MVP that enables doctors to manage appointments and send automated reminders to patients. The system will support role-based access for doctors and clinic administrators, provide calendar-based appointment scheduling, maintain basic patient records, and integrate with WhatsApp Cloud API and SendGrid for automated notifications.

## Requirements

### Requirement 1

**User Story:** As a doctor, I want to authenticate securely into the system, so that I can access my appointment management features with proper authorization.

#### Acceptance Criteria

1. WHEN a doctor enters valid credentials THEN the system SHALL authenticate using JWT tokens
2. WHEN authentication is successful THEN the system SHALL provide role-based access based on user type (Doctor/Admin)
3. WHEN a doctor logs in THEN the system SHALL restrict access to only their own appointments
4. IF authentication fails THEN the system SHALL return appropriate error messages

### Requirement 2

**User Story:** As a doctor, I want to view my appointments in a calendar format, so that I can easily see my schedule and manage my time effectively.

#### Acceptance Criteria

1. WHEN a doctor accesses the dashboard THEN the system SHALL display appointments in a FullCalendar view
2. WHEN viewing the calendar THEN the system SHALL show appointment details including patient name, time, and status
3. WHEN a doctor clicks on an appointment THEN the system SHALL display detailed appointment information
4. WHEN viewing appointments THEN the system SHALL only show appointments assigned to the logged-in doctor

### Requirement 3

**User Story:** As a doctor, I want to create new appointments for patients, so that I can schedule consultations and manage my patient load.

#### Acceptance Criteria

1. WHEN creating an appointment THEN the system SHALL require patient name, phone, email, date, time, and status
2. WHEN scheduling an appointment THEN the system SHALL prevent double booking by enforcing unique constraint on doctor_id + datetime
3. WHEN an appointment is created THEN the system SHALL link it to the appropriate patient record
4. IF a time slot is already booked THEN the system SHALL reject the appointment and display an error message

### Requirement 4

**User Story:** As a doctor, I want to update or cancel existing appointments, so that I can accommodate schedule changes and patient needs.

#### Acceptance Criteria

1. WHEN updating an appointment THEN the system SHALL allow modification of patient details, date, time, and status
2. WHEN canceling an appointment THEN the system SHALL update the status to canceled
3. WHEN modifying appointment time THEN the system SHALL validate against double booking constraints
4. WHEN changes are made THEN the system SHALL update the appointment record in the database

### Requirement 5

**User Story:** As a clinic administrator, I want to view and manage all appointments across all doctors, so that I can oversee clinic operations and assist with scheduling conflicts.

#### Acceptance Criteria

1. WHEN an admin logs in THEN the system SHALL provide access to all appointments regardless of doctor
2. WHEN viewing appointments as admin THEN the system SHALL display doctor information alongside appointment details
3. WHEN managing appointments as admin THEN the system SHALL allow full CRUD operations on any appointment
4. WHEN accessing admin features THEN the system SHALL verify admin role authorization

### Requirement 6

**User Story:** As a doctor, I want to maintain basic patient records, so that I can store contact information and notes for future reference.

#### Acceptance Criteria

1. WHEN creating a patient record THEN the system SHALL store name, phone, email, and notes
2. WHEN viewing patient records THEN the system SHALL display linked appointments for each patient
3. WHEN updating patient information THEN the system SHALL reflect changes across all related appointments
4. WHEN searching patients THEN the system SHALL provide filtering capabilities by name or contact information

### Requirement 7

**User Story:** As a patient, I want to receive automatic appointment reminders, so that I don't miss my scheduled consultations.

#### Acceptance Criteria

1. WHEN an appointment is 24 hours away THEN the system SHALL automatically send a reminder via WhatsApp Cloud API
2. IF WhatsApp delivery fails THEN the system SHALL send a fallback email reminder via SendGrid
3. WHEN sending reminders THEN the system SHALL include appointment details (date, time, doctor name, clinic address)
4. WHEN reminders are sent THEN the system SHALL log the delivery status for tracking

### Requirement 8

**User Story:** As a system administrator, I want background job processing for reminders, so that notifications are sent reliably without impacting application performance.

#### Acceptance Criteria

1. WHEN the system starts THEN Hangfire SHALL initialize background job processing
2. WHEN appointments are created THEN the system SHALL schedule reminder jobs for 24 hours before appointment time
3. WHEN reminder jobs execute THEN the system SHALL process notifications asynchronously
4. IF reminder jobs fail THEN the system SHALL implement retry logic with exponential backoff

### Requirement 9

**User Story:** As a developer, I want a modular and scalable architecture, so that the system can be extended for hospital-level operations in the future.

#### Acceptance Criteria

1. WHEN implementing the backend THEN the system SHALL use .NET 8 Minimal APIs with clean architecture principles
2. WHEN implementing the frontend THEN the system SHALL use React with TailwindCSS for responsive design
3. WHEN designing the database THEN the system SHALL use PostgreSQL with proper normalization and indexing
4. WHEN structuring the code THEN the system SHALL implement separation of concerns and dependency injection

### Requirement 10

**User Story:** As a system operator, I want the application to be deployable to cloud platforms, so that it can be accessed reliably and scaled as needed.

#### Acceptance Criteria

1. WHEN deploying the backend THEN the system SHALL be compatible with Azure App Service (Linux)
2. WHEN deploying the frontend THEN the system SHALL be compatible with Vercel hosting
3. WHEN configuring the system THEN environment variables SHALL be used for API keys and connection strings
4. WHEN setting up integrations THEN the system SHALL provide sample configuration for WhatsApp Cloud API and SendGrid