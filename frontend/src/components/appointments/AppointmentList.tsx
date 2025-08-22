import { useState, useEffect } from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { format } from 'date-fns';
import { 
  MoreHorizontal, 
  Edit, 
  Trash2, 
  CheckCircle, 
  XCircle, 
  Clock,
  Heart,
  Shield,
  Calendar,
  User,
  Phone,
  Mail
} from 'lucide-react';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { appointmentService } from '@/services/appointments';
import { useAuth } from '@/hooks/useAuth';
import { toast } from 'sonner';
import type { Appointment, AppointmentStatus } from '@/types';

interface AppointmentListProps {
  onEdit?: (appointment: Appointment) => void;
  refreshTrigger?: number;
}

export function AppointmentList({ onEdit, refreshTrigger }: AppointmentListProps) {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [updatingStatus, setUpdatingStatus] = useState<number | null>(null);

  useEffect(() => {
    loadAppointments();
  }, [user, refreshTrigger]);

  const loadAppointments = async () => {
    try {
      setLoading(true);
      
      let appointmentData: Appointment[];
      
      // Role-based filtering: doctors see only their appointments
      if (user?.role === 'Doctor' && user.doctorId) {
        appointmentData = await appointmentService.getAppointmentsByDoctor(user.doctorId);
      } else {
        // Admin sees all appointments
        appointmentData = await appointmentService.getAppointments();
      }
      
      setAppointments(appointmentData);
    } catch (error) {
      toast.error('Failed to load appointments');
      console.error('Error loading appointments:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (appointmentId: number, newStatus: AppointmentStatus) => {
    try {
      setUpdatingStatus(appointmentId);
      await appointmentService.updateAppointment(appointmentId, { status: newStatus } as any);
      
      // Update local state
      setAppointments(prev => 
        prev.map(apt => 
          apt.id === appointmentId 
            ? { ...apt, status: newStatus }
            : apt
        )
      );
      
      toast.success(`Appointment marked as ${newStatus.toLowerCase()}`);
    } catch (error) {
      toast.error('Failed to update appointment status');
      console.error('Error updating appointment:', error);
    } finally {
      setUpdatingStatus(null);
    }
  };

  const handleDelete = async (appointmentId: number) => {
    if (!confirm('Are you sure you want to delete this appointment?')) {
      return;
    }

    try {
      await appointmentService.deleteAppointment(appointmentId);
      setAppointments(prev => prev.filter(apt => apt.id !== appointmentId));
      toast.success('Appointment deleted successfully');
    } catch (error) {
      toast.error('Failed to delete appointment');
      console.error('Error deleting appointment:', error);
    }
  };

  const getStatusBadge = (status: AppointmentStatus) => {
    const variants = {
      Scheduled: { variant: 'default' as const, icon: Clock, color: 'text-teal-600' },
      Completed: { variant: 'secondary' as const, icon: CheckCircle, color: 'text-green-600' },
      Cancelled: { variant: 'destructive' as const, icon: XCircle, color: 'text-red-600' },
      NoShow: { variant: 'outline' as const, icon: XCircle, color: 'text-amber-600' },
    };

    const config = variants[status];
    const Icon = config.icon;

    return (
      <Badge variant={config.variant} className="flex items-center space-x-1">
        <Icon className={`h-3 w-3 ${config.color}`} />
        <span>{status === 'NoShow' ? 'No Show' : status}</span>
      </Badge>
    );
  };

  const columns: ColumnDef<Appointment>[] = [
    {
      accessorKey: "patient.name",
      header: "Patient",
      cell: ({ row }) => {
        const appointment = row.original;
        return (
          <div className="flex items-center space-x-2">
            <div className="p-1 bg-teal-100 dark:bg-teal-900/20 rounded-full">
              <User className="h-3 w-3 text-teal-600" />
            </div>
            <div>
              <div className="font-medium">{appointment.patient?.name || 'Unknown'}</div>
              {appointment.patient?.phone && (
                <div className="text-xs text-muted-foreground flex items-center space-x-1">
                  <Phone className="h-3 w-3" />
                  <span>{appointment.patient.phone}</span>
                </div>
              )}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "dateTime",
      header: "Date & Time",
      cell: ({ row }) => {
        const dateTime = new Date(row.getValue("dateTime"));
        return (
          <div className="flex items-center space-x-2">
            <Calendar className="h-4 w-4 text-teal-500" />
            <div>
              <div className="font-medium">{format(dateTime, 'MMM dd, yyyy')}</div>
              <div className="text-sm text-muted-foreground">{format(dateTime, 'h:mm a')}</div>
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => getStatusBadge(row.getValue("status")),
    },
    ...(user?.role === 'Admin' ? [{
      accessorKey: "doctor.name",
      header: "Doctor",
      cell: ({ row }: { row: any }) => {
        const appointment = row.original;
        return (
          <div className="flex items-center space-x-2">
            <Shield className="h-4 w-4 text-teal-500" />
            <span>{appointment.doctor?.name || 'Unknown'}</span>
          </div>
        );
      },
    }] : []),
    {
      id: "actions",
      enableHiding: false,
      cell: ({ row }) => {
        const appointment = row.original;
        const isUpdating = updatingStatus === appointment.id;

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button 
                variant="ghost" 
                className="h-8 w-8 p-0 hover:bg-teal-50 dark:hover:bg-teal-900/20"
                disabled={isUpdating}
              >
                <span className="sr-only">Open menu</span>
                {isUpdating ? (
                  <div className="h-4 w-4 animate-spin rounded-full border-2 border-teal-500 border-t-transparent" />
                ) : (
                  <MoreHorizontal className="h-4 w-4" />
                )}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="medical-card">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuItem
                onClick={() => {
                  setSelectedAppointment(appointment);
                  setIsDetailDialogOpen(true);
                }}
                className="hover:bg-teal-50 dark:hover:bg-teal-900/20"
              >
                <Heart className="mr-2 h-4 w-4 text-teal-500" />
                View Details
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => onEdit?.(appointment)}
                className="hover:bg-teal-50 dark:hover:bg-teal-900/20"
              >
                <Edit className="mr-2 h-4 w-4 text-teal-500" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              {appointment.status === 'Scheduled' && (
                <>
                  <DropdownMenuItem
                    onClick={() => handleStatusUpdate(appointment.id, 'Completed')}
                    className="hover:bg-green-50 dark:hover:bg-green-900/20"
                  >
                    <CheckCircle className="mr-2 h-4 w-4 text-green-500" />
                    Mark Completed
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => handleStatusUpdate(appointment.id, 'NoShow')}
                    className="hover:bg-amber-50 dark:hover:bg-amber-900/20"
                  >
                    <XCircle className="mr-2 h-4 w-4 text-amber-500" />
                    Mark No Show
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => handleStatusUpdate(appointment.id, 'Cancelled')}
                    className="hover:bg-red-50 dark:hover:bg-red-900/20"
                  >
                    <XCircle className="mr-2 h-4 w-4 text-red-500" />
                    Cancel
                  </DropdownMenuItem>
                </>
              )}
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => handleDelete(appointment.id)}
                className="text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];

  if (loading) {
    return (
      <Card className="medical-card animate-medical-pulse">
        <CardContent className="flex items-center justify-center h-64">
          <div className="flex items-center space-x-2 text-muted-foreground">
            <div className="h-6 w-6 animate-spin rounded-full border-2 border-teal-500 border-t-transparent" />
            <span>Loading appointments...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card className="medical-card animate-fade-in">
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <Heart className="h-5 w-5 text-teal-500 animate-medical-sparkle" />
            <span>Appointment List</span>
            {user?.role === 'Doctor' && (
              <Badge variant="outline" className="ml-auto">
                My Appointments
              </Badge>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={appointments}
            searchKey="patient.name"
            searchPlaceholder="Search patients..."
          />
        </CardContent>
      </Card>

      {/* Appointment Detail Dialog */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="medical-card max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center space-x-2">
              <Heart className="h-5 w-5 text-teal-500" />
              <span>Appointment Details</span>
            </DialogTitle>
            <DialogDescription>
              Complete information about this appointment
            </DialogDescription>
          </DialogHeader>
          
          {selectedAppointment && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="font-medium">Status:</span>
                {getStatusBadge(selectedAppointment.status)}
              </div>
              
              <div className="space-y-3">
                <div className="flex items-center space-x-3">
                  <User className="h-4 w-4 text-teal-500" />
                  <div>
                    <div className="font-medium">{selectedAppointment.patient?.name || 'Unknown'}</div>
                    <div className="text-sm text-muted-foreground">Patient</div>
                  </div>
                </div>
                
                <div className="flex items-center space-x-3">
                  <Calendar className="h-4 w-4 text-teal-500" />
                  <div>
                    <div className="font-medium">
                      {format(new Date(selectedAppointment.dateTime), 'EEEE, MMMM dd, yyyy')}
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {format(new Date(selectedAppointment.dateTime), 'h:mm a')}
                    </div>
                  </div>
                </div>
                
                {selectedAppointment.patient?.phone && (
                  <div className="flex items-center space-x-3">
                    <Phone className="h-4 w-4 text-teal-500" />
                    <div>
                      <div className="font-medium">{selectedAppointment.patient.phone}</div>
                      <div className="text-sm text-muted-foreground">Phone</div>
                    </div>
                  </div>
                )}
                
                {selectedAppointment.patient?.email && (
                  <div className="flex items-center space-x-3">
                    <Mail className="h-4 w-4 text-teal-500" />
                    <div>
                      <div className="font-medium">{selectedAppointment.patient.email}</div>
                      <div className="text-sm text-muted-foreground">Email</div>
                    </div>
                  </div>
                )}
                
                {user?.role === 'Admin' && selectedAppointment.doctor && (
                  <div className="flex items-center space-x-3">
                    <Shield className="h-4 w-4 text-teal-500" />
                    <div>
                      <div className="font-medium">{selectedAppointment.doctor.name}</div>
                      <div className="text-sm text-muted-foreground">Doctor</div>
                    </div>
                  </div>
                )}
                
                {selectedAppointment.notes && (
                  <div className="space-y-1">
                    <div className="font-medium">Notes:</div>
                    <div className="text-sm text-muted-foreground bg-muted/50 p-3 rounded-lg">
                      {selectedAppointment.notes}
                    </div>
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