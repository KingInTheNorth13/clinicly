import { useState } from 'react';
import { AppointmentList } from '@/components/appointments/AppointmentList';
import { AppointmentForm } from '@/components/appointments/AppointmentForm';
import { CalendarView } from '@/components/appointments/CalendarView';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Plus, Calendar, List } from 'lucide-react';

export function AppointmentsPage() {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Appointments</h2>
          <p className="text-muted-foreground">
            Manage patient appointments and schedules
          </p>
        </div>
        <AppointmentForm 
          open={isCreateDialogOpen} 
          onOpenChange={setIsCreateDialogOpen}
        />
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Appointment
        </Button>
      </div>

      <Tabs defaultValue="calendar" className="space-y-4">
        <TabsList>
          <TabsTrigger value="calendar">
            <Calendar className="mr-2 h-4 w-4" />
            Calendar View
          </TabsTrigger>
          <TabsTrigger value="list">
            <List className="mr-2 h-4 w-4" />
            List View
          </TabsTrigger>
        </TabsList>
        
        <TabsContent value="calendar" className="space-y-4">
          <CalendarView />
        </TabsContent>
        
        <TabsContent value="list" className="space-y-4">
          <AppointmentList />
        </TabsContent>
      </Tabs>
    </div>
  );
}