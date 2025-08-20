import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export function AppointmentsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Appointments</h2>
        <p className="text-muted-foreground">
          Manage your appointments and schedule
        </p>
      </div>
      
      <Card>
        <CardHeader>
          <CardTitle>Appointment Calendar</CardTitle>
          <CardDescription>
            View and manage your appointments in calendar format
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="h-96 flex items-center justify-center border-2 border-dashed border-muted-foreground/25 rounded-lg">
            <p className="text-muted-foreground">Calendar component will be implemented here</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}