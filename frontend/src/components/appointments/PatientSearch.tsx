import { useState, useEffect } from 'react';
import { Check, ChevronsUpDown, User, Plus, Search } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Badge } from '@/components/ui/badge';
import type { Patient } from '@/types';

interface PatientSearchProps {
  selectedPatient?: Patient | null;
  onPatientSelect: (patient: Patient | null) => void;
  onCreateNew?: () => void;
  disabled?: boolean;
  className?: string;
}

export function PatientSearch({
  selectedPatient,
  onPatientSelect,
  onCreateNew,
  disabled = false,
  className
}: PatientSearchProps) {
  const [open, setOpen] = useState(false);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  // Mock data for now - will be replaced with actual API call
  const mockPatients: Patient[] = [
    {
      id: 1,
      clinicId: 1,
      name: 'John Doe',
      phone: '+1 (555) 123-4567',
      email: 'john.doe@email.com',
      notes: 'Regular patient',
      createdAt: '2024-01-15T10:00:00Z'
    },
    {
      id: 2,
      clinicId: 1,
      name: 'Jane Smith',
      phone: '+1 (555) 987-6543',
      email: 'jane.smith@email.com',
      notes: 'Allergic to penicillin',
      createdAt: '2024-01-14T14:30:00Z'
    },
    {
      id: 3,
      clinicId: 1,
      name: 'Robert Johnson',
      phone: '+1 (555) 456-7890',
      email: 'robert.j@email.com',
      createdAt: '2024-01-13T09:15:00Z'
    }
  ];

  useEffect(() => {
    // Simulate API call
    setLoading(true);
    setTimeout(() => {
      setPatients(mockPatients);
      setLoading(false);
    }, 300);
  }, []);

  const filteredPatients = patients.filter(patient =>
    patient.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    patient.email?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    patient.phone?.includes(searchQuery)
  );

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className={cn(
            "medical-input justify-between",
            !selectedPatient && "text-muted-foreground",
            className
          )}
          disabled={disabled}
        >
          <div className="flex items-center space-x-2">
            <User className="h-4 w-4 text-teal-500" />
            {selectedPatient ? (
              <div className="flex items-center space-x-2">
                <span>{selectedPatient.name}</span>
                {selectedPatient.phone && (
                  <Badge variant="outline" className="text-xs">
                    {selectedPatient.phone}
                  </Badge>
                )}
              </div>
            ) : (
              <span>Select patient...</span>
            )}
          </div>
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="medical-card w-80 p-0">
        <Command>
          <div className="flex items-center border-b px-3">
            <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
            <CommandInput
              placeholder="Search patients..."
              value={searchQuery}
              onValueChange={setSearchQuery}
              className="flex h-11 w-full rounded-md bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
          <CommandList>
            <CommandEmpty className="py-6 text-center text-sm">
              <div className="flex flex-col items-center space-y-2">
                <User className="h-8 w-8 text-muted-foreground" />
                <span>No patients found.</span>
                {onCreateNew && (
                  <Button
                    size="sm"
                    onClick={() => {
                      setOpen(false);
                      onCreateNew();
                    }}
                    className="medical-button"
                  >
                    <Plus className="h-4 w-4 mr-2" />
                    Create New Patient
                  </Button>
                )}
              </div>
            </CommandEmpty>
            <CommandGroup>
              {loading ? (
                <CommandItem disabled>
                  <div className="flex items-center space-x-2">
                    <div className="h-4 w-4 animate-spin rounded-full border-2 border-teal-500 border-t-transparent" />
                    <span>Loading patients...</span>
                  </div>
                </CommandItem>
              ) : (
                <>
                  {filteredPatients.map((patient) => (
                    <CommandItem
                      key={patient.id}
                      value={patient.name}
                      onSelect={() => {
                        onPatientSelect(patient.id === selectedPatient?.id ? null : patient);
                        setOpen(false);
                      }}
                      className="cursor-pointer hover:bg-teal-50 dark:hover:bg-teal-900/20"
                    >
                      <div className="flex items-center justify-between w-full">
                        <div className="flex items-center space-x-2">
                          <Check
                            className={cn(
                              "h-4 w-4 text-teal-500",
                              selectedPatient?.id === patient.id ? "opacity-100" : "opacity-0"
                            )}
                          />
                          <div className="flex flex-col">
                            <span className="font-medium">{patient.name}</span>
                            <div className="flex items-center space-x-2 text-xs text-muted-foreground">
                              {patient.phone && <span>{patient.phone}</span>}
                              {patient.email && <span>{patient.email}</span>}
                            </div>
                          </div>
                        </div>
                      </div>
                    </CommandItem>
                  ))}
                  {onCreateNew && (
                    <CommandItem
                      onSelect={() => {
                        setOpen(false);
                        onCreateNew();
                      }}
                      className="cursor-pointer border-t border-border mt-2 pt-2 hover:bg-teal-50 dark:hover:bg-teal-900/20"
                    >
                      <Plus className="h-4 w-4 mr-2 text-teal-500" />
                      <span className="text-teal-600 dark:text-teal-400 font-medium">
                        Create New Patient
                      </span>
                    </CommandItem>
                  )}
                </>
              )}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}