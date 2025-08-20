import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export function PatientsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Patients</h2>
        <p className="text-muted-foreground">
          Manage patient records and information
        </p>
      </div>
      
      <Card>
        <CardHeader>
          <CardTitle>Patient List</CardTitle>
          <CardDescription>
            View and manage patient records
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="h-96 flex items-center justify-center border-2 border-dashed border-muted-foreground/25 rounded-lg">
            <p className="text-muted-foreground">Patient list component will be implemented here</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}