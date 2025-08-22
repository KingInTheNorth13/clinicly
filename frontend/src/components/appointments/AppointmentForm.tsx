import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Calendar, Clock, Shield, Lock, Stethoscope, Save, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';

import { Textarea } from '@/components/ui/textarea';
import { DatePicker } from '@/components/ui/date-picker';
import { TimePicker } from '@/components/ui/time-picker';
import { PatientSearch } from './PatientSearch';
import { Badge } from '@/components/ui/badge';
import { appointmentService } from '@/services/appointments';
import type { Appointment, Patient, AppointmentFormData } from '@/types';

const appointmentFormSchema = z.object({
  patientId: z.number().min(1, 'Please select a patient'),
  date: z.date({
    message: 'Please select a date',
  }),
  time: z.string().min(1, 'Please select a time'),
  notes: z.string().optional(),
});

type AppointmentFormValues = z.infer<typeof appointmentFormSchema>;

interface AppointmentFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  appointment?: Appointment;
  selectedDate?: Date;
  onSuccess?: (appointment: Appointment) => void;
  onError?: (error: string) => void;
}

export function AppointmentForm({
  open,
  onOpenChange,
  appointment,
  selectedDate,
  onSuccess,
  onError
}: AppointmentFormProps) {
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [loading, setLoading] = useState(false);
  const [showPatientForm, setShowPatientForm] = useState(false);

  const form = useForm<AppointmentFormValues>({
    resolver: zodResolver(appointmentFormSchema),
    defaultValues: {
      patientId: 0,
      date: selectedDate || new Date(),
      time: '',
      notes: '',
    },
  });

  const isEditing = !!appointment;

  useEffect(() => {
    if (appointment) {
      const appointmentDate = new Date(appointment.dateTime);
      const timeString = appointmentDate.toTimeString().slice(0, 5);
      
      form.reset({
        patientId: appointment.patientId,
        date: appointmentDate,
        time: timeString,
        notes: appointment.notes || '',
      });
      
      if (appointment.patient) {
        setSelectedPatient(appointment.patient);
      }
    } else if (selectedDate) {
      form.setValue('date', selectedDate);
    }
  }, [appointment, selectedDate, form]);

  const onSubmit = async (values: AppointmentFormValues) => {
    try {
      setLoading(true);
      
      // Combine date and time
      const dateTime = new Date(values.date);
      const [hours, minutes] = values.time.split(':').map(Number);
      dateTime.setHours(hours, minutes, 0, 0);

      const formData: AppointmentFormData = {
        patientId: values.patientId,
        dateTime: dateTime.toISOString(),
        notes: values.notes,
      };

      let result: Appointment;
      if (isEditing && appointment) {
        result = await appointmentService.updateAppointment(appointment.id, formData);
      } else {
        result = await appointmentService.createAppointment(formData);
      }

      onSuccess?.(result);
      onOpenChange(false);
      form.reset();
      setSelectedPatient(null);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to save appointment';
      onError?.(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handlePatientSelect = (patient: Patient | null) => {
    setSelectedPatient(patient);
    form.setValue('patientId', patient?.id || 0);
  };

  const handleCreateNewPatient = () => {
    setShowPatientForm(true);
  };

  const handleClose = () => {
    onOpenChange(false);
    form.reset();
    setSelectedPatient(null);
  };

  return (
    <>
      <Dialog open={open} onOpenChange={handleClose}>
        <DialogContent className="medical-card max-w-2xl animate-slide-up">
          <DialogHeader className="space-y-3">
            <DialogTitle className="flex items-center space-x-2 text-xl">
              <div className="p-2 bg-gradient-to-r from-teal-500 to-cyan-600 rounded-xl animate-medical-pulse">
                <Stethoscope className="h-5 w-5 text-white" />
              </div>
              <span>{isEditing ? 'Edit Appointment' : 'New Appointment'}</span>
              <Badge variant="outline" className="ml-auto">
                <Shield className="h-3 w-3 mr-1" />
                Secure
              </Badge>
            </DialogTitle>
            <DialogDescription className="text-muted-foreground">
              {isEditing 
                ? 'Update the appointment details below.' 
                : 'Schedule a new appointment for your patient.'
              }
            </DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Patient Selection */}
                <div className="md:col-span-2">
                  <FormField
                    control={form.control}
                    name="patientId"
                    render={() => (
                      <FormItem>
                        <FormLabel className="flex items-center space-x-2">
                          <Lock className="h-4 w-4 text-teal-500" />
                          <span>Patient</span>
                        </FormLabel>
                        <FormControl>
                          <PatientSearch
                            selectedPatient={selectedPatient}
                            onPatientSelect={handlePatientSelect}
                            onCreateNew={handleCreateNewPatient}
                            disabled={loading}
                          />
                        </FormControl>
                        <FormDescription>
                          Search and select a patient or create a new one.
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                {/* Date Selection */}
                <FormField
                  control={form.control}
                  name="date"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center space-x-2">
                        <Calendar className="h-4 w-4 text-teal-500" />
                        <span>Date</span>
                      </FormLabel>
                      <FormControl>
                        <DatePicker
                          date={field.value}
                          onDateChange={field.onChange}
                          disabled={loading}
                          className="w-full"
                        />
                      </FormControl>
                      <FormDescription>
                        Select the appointment date.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Time Selection */}
                <FormField
                  control={form.control}
                  name="time"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center space-x-2">
                        <Clock className="h-4 w-4 text-teal-500" />
                        <span>Time</span>
                      </FormLabel>
                      <FormControl>
                        <TimePicker
                          time={field.value}
                          onTimeChange={field.onChange}
                          disabled={loading}
                          className="w-full"
                        />
                      </FormControl>
                      <FormDescription>
                        Select the appointment time.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Notes */}
              <FormField
                control={form.control}
                name="notes"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Notes (Optional)</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Add any additional notes about the appointment..."
                        className="medical-input min-h-[100px] resize-none"
                        disabled={loading}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Any additional information about the appointment.
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <DialogFooter className="flex items-center space-x-2 pt-4 border-t border-border">
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleClose}
                  disabled={loading}
                  className="flex items-center space-x-2"
                >
                  <X className="h-4 w-4" />
                  <span>Cancel</span>
                </Button>
                <Button
                  type="submit"
                  disabled={loading}
                  className="medical-button flex items-center space-x-2 animate-medical-float"
                >
                  {loading ? (
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                  ) : (
                    <Save className="h-4 w-4" />
                  )}
                  <span>{loading ? 'Saving...' : (isEditing ? 'Update' : 'Create')}</span>
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Patient Form Dialog - Placeholder for now */}
      <Dialog open={showPatientForm} onOpenChange={setShowPatientForm}>
        <DialogContent className="medical-card">
          <DialogHeader>
            <DialogTitle>Create New Patient</DialogTitle>
            <DialogDescription>
              Patient creation form will be implemented in the next task.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button onClick={() => setShowPatientForm(false)}>Close</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}