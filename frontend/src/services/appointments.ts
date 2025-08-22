import { apiService } from './api';
import type { Appointment, AppointmentFormData, CalendarEvent } from '@/types';

export class AppointmentService {
  async getAppointments(): Promise<Appointment[]> {
    const response = await apiService.get<Appointment[]>('/appointments');
    return response || [];
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

  private getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
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