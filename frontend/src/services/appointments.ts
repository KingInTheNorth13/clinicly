import { apiService } from './api';
import type { Appointment, AppointmentFormData, CalendarEvent, AppointmentStatus } from '@/types';

export class AppointmentService {
  async getAppointments(): Promise<Appointment[]> {
    // Use the search endpoint with default parameters to get all appointments
    const searchRequest = {
      page: 1,
      pageSize: 1000 // Large page size to get all appointments
    };
    
    // Try admin endpoint first, fall back to regular search if not admin
    try {
      const response = await apiService.post<{items: Appointment[], totalCount: number}>('/appointments/admin/search', searchRequest);
      return response?.items || [];
    } catch (error) {
      // If admin endpoint fails, try regular search endpoint
      const response = await apiService.post<{items: Appointment[], totalCount: number}>('/appointments/search', searchRequest);
      return response?.items || [];
    }
  }

  async getAppointmentsByDoctor(doctorId: number): Promise<Appointment[]> {
    const response = await apiService.get<Appointment[]>(`/appointments/doctor/${doctorId}`);
    return response || [];
  }

  async getAppointment(id: number): Promise<Appointment> {
    const response = await apiService.get<Appointment>(`/appointments/${id}`);
    if (!response) {
      throw new Error('Appointment not found');
    }
    return response;
  }

  async createAppointment(data: AppointmentFormData): Promise<Appointment> {
    const response = await apiService.post<Appointment>('/appointments', data);
    if (!response) {
      throw new Error('Failed to create appointment');
    }
    return response;
  }

  async updateAppointment(id: number, data: Partial<AppointmentFormData>): Promise<Appointment> {
    const response = await apiService.put<Appointment>(`/appointments/${id}`, data);
    if (!response) {
      throw new Error('Failed to update appointment');
    }
    return response;
  }

  async deleteAppointment(id: number): Promise<void> {
    await apiService.delete(`/appointments/${id}`);
  }

  // Convert appointments to FullCalendar events
  appointmentsToCalendarEvents(appointments: Appointment[]): CalendarEvent[] {
    return appointments.map(appointment => ({
      id: appointment.id.toString(),
      title: appointment.patient?.name || 'Unknown Patient',
      start: appointment.dateTime,
      end: this.calculateEndTime(appointment.dateTime),
      backgroundColor: this.getStatusColor(appointment.status),
      borderColor: this.getStatusColor(appointment.status),
      extendedProps: {
        appointment
      }
    }));
  }

  private calculateEndTime(startTime: string): string {
    const start = new Date(startTime);
    const end = new Date(start.getTime() + 60 * 60 * 1000); // Add 1 hour
    return end.toISOString();
  }

  private getStatusColor(status: AppointmentStatus): string {
    // Handle null/undefined status
    if (!status) {
      return 'hsl(var(--medical-primary))';
    }
    
    // Convert to string and lowercase for comparison
    const statusStr = status.toString().toLowerCase();
    
    switch (statusStr) {
      case 'scheduled':
        return 'hsl(var(--medical-primary))';
      case 'completed':
        return 'hsl(var(--medical-success))';
      case 'cancelled':
        return 'hsl(var(--medical-error))';
      case 'noshow':
        return 'hsl(var(--medical-warning))';
      default:
        return 'hsl(var(--medical-primary))';
    }
  }
}

export const appointmentService = new AppointmentService();