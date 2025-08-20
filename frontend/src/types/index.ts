// Core entity types
export interface User {
  id: number;
  email: string;
  role: 'Doctor' | 'Admin';
  doctorId?: number;
  clinicId: number;
}

export interface Doctor {
  id: number;
  clinicId: number;
  name: string;
  specialization?: string;
  email: string;
  createdAt: string;
}

export interface Patient {
  id: number;
  clinicId: number;
  name: string;
  phone?: string;
  email?: string;
  notes?: string;
  createdAt: string;
}

export interface Appointment {
  id: number;
  doctorId: number;
  patientId: number;
  dateTime: string;
  status: AppointmentStatus;
  notes?: string;
  createdAt: string;
  doctor?: Doctor;
  patient?: Patient;
}

export type AppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow';

export interface Clinic {
  id: number;
  name: string;
  address?: string;
  createdAt: string;
}

// Authentication types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// API response types
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: ApiError;
  timestamp: string;
  correlationId?: string;
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, any>;
}

// Form types
export interface AppointmentFormData {
  patientId: number;
  dateTime: string;
  notes?: string;
}

export interface PatientFormData {
  name: string;
  phone?: string;
  email?: string;
  notes?: string;
}

// Calendar event type for FullCalendar
export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end?: string;
  backgroundColor?: string;
  borderColor?: string;
  extendedProps?: {
    appointment: Appointment;
  };
}