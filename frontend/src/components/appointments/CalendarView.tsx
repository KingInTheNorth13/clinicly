import { useState, useEffect } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import { Calendar, Stethoscope, Heart } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { appointmentService } from '@/services/appointments';
import { useAuth } from '@/hooks/useAuth';
import type { Appointment, CalendarEvent } from '@/types';

interface CalendarViewProps {
  onAppointmentSelect?: (appointment: Appointment) => void;
  onDateSelect?: (date: Date) => void;
}

export function CalendarView({ onAppointmentSelect, onDateSelect }: CalendarViewProps) {
  const { user } = useAuth();
  const [calendarEvents, setCalendarEvents] = useState<CalendarEvent[]>([]);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadAppointments();
  }, [user]);

  const loadAppointments = async () => {
    try {
      setLoading(true);
      setError(null);
      
      let appointmentData: Appointment[];
      
      // Role-based filtering: doctors see only their appointments
      if (user?.role === 'Doctor' && user.doctorId) {
        appointmentData = await appointmentService.getAppointmentsByDoctor(user.doctorId);
      } else {
        // Admin sees all appointments
        appointmentData = await appointmentService.getAppointments();
      }
      
      setCalendarEvents(appointmentService.appointmentsToCalendarEvents(appointmentData));
    } catch (err) {
      setError('Failed to load appointments');
      console.error('Error loading appointments:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleEventClick = (clickInfo: any) => {
    const appointment = clickInfo.event.extendedProps.appointment as Appointment;
    setSelectedAppointment(appointment);
    setIsDetailDialogOpen(true);
    onAppointmentSelect?.(appointment);
  };

  const handleDateClick = (dateClickInfo: any) => {
    const clickedDate = new Date(dateClickInfo.date);
    onDateSelect?.(clickedDate);
  };

  const getStatusBadgeVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case 'scheduled':
        return 'default';
      case 'completed':
        return 'secondary';
      case 'cancelled':
        return 'destructive';
      case 'noshow':
        return 'outline';
      default:
        return 'default';
    }
  };

  const formatDateTime = (dateTime: string) => {
    return new Date(dateTime).toLocaleString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  if (loading) {
    return (
      <Card className="medical-card animate-medical-pulse">
        <CardContent className="flex items-center justify-center h-96">
          <div className="flex items-center space-x-2 text-muted-foreground">
            <Stethoscope className="h-6 w-6 animate-medical-float" />
            <span>Loading appointments...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="medical-card border-red-200 dark:border-red-800">
        <CardContent className="flex items-center justify-center h-96">
          <div className="text-center">
            <Heart className="h-12 w-12 text-red-500 mx-auto mb-4" />
            <p className="text-red-600 dark:text-red-400 font-medium">{error}</p>
            <Button 
              onClick={loadAppointments} 
              className="medical-button mt-4"
            >
              Try Again
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card className="medical-card animate-fade-in">
        <CardHeader className="pb-4">
          <CardTitle className="flex items-center space-x-2 text-xl font-bold">
            <Calendar className="h-6 w-6 text-teal-500 animate-medical-sparkle" />
            <span>Appointment Calendar</span>
            {user?.role === 'Doctor' && (
              <Badge variant="outline" className="ml-auto">
                My Appointments
              </Badge>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="calendar-container">
            <FullCalendar
              plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
              initialView="dayGridMonth"
              headerToolbar={{
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
              }}
              events={calendarEvents}
              eventClick={handleEventClick}
              dateClick={handleDateClick}
              height="auto"
              eventDisplay="block"
              dayMaxEvents={3}
              moreLinkClick="popover"
              eventClassNames={(arg) => {
                const status = arg.event.extendedProps.appointment.status.toLowerCase();
                return [`fc-event-${status}`];
              }}
              dayCellClassNames="hover:bg-muted/50 transition-colors duration-200"
              eventMouseEnter={(info) => {
                info.el.style.transform = 'translateY(-2px)';
                info.el.style.boxShadow = '0 8px 16px rgba(0, 0, 0, 0.2)';
              }}
              eventMouseLeave={(info) => {
                info.el.style.transform = 'translateY(0)';
                info.el.style.boxShadow = '0 2px 4px rgba(0, 0, 0, 0.1)';
              }}
            />
          </div>
        </CardContent>
      </Card>

      {/* Appointment Detail Dialog */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="medical-card max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center space-x-2">
              <Stethoscope className="h-5 w-5 text-teal-500" />
              <span>Appointment Details</span>
            </DialogTitle>
          </DialogHeader>
          
          {selectedAppointment && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="font-medium">Status:</span>
                <Badge variant={getStatusBadgeVariant(selectedAppointment.status)}>
                  {selectedAppointment.status}
                </Badge>
              </div>
              
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-medium">Patient:</span>
                  <span>{selectedAppointment.patient?.name || 'Unknown'}</span>
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="font-medium">Date & Time:</span>
                  <span className="text-sm">{formatDateTime(selectedAppointment.dateTime)}</span>
                </div>
                
                {selectedAppointment.patient?.phone && (
                  <div className="flex items-center justify-between">
                    <span className="font-medium">Phone:</span>
                    <span>{selectedAppointment.patient.phone}</span>
                  </div>
                )}
                
                {selectedAppointment.patient?.email && (
                  <div className="flex items-center justify-between">
                    <span className="font-medium">Email:</span>
                    <span className="text-sm">{selectedAppointment.patient.email}</span>
                  </div>
                )}
                
                {user?.role === 'Admin' && selectedAppointment.doctor && (
                  <div className="flex items-center justify-between">
                    <span className="font-medium">Doctor:</span>
                    <span>{selectedAppointment.doctor.name}</span>
                  </div>
                )}
                
                {selectedAppointment.notes && (
                  <div className="space-y-1">
                    <span className="font-medium">Notes:</span>
                    <p className="text-sm text-muted-foreground bg-muted/50 p-3 rounded-lg">
                      {selectedAppointment.notes}
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}