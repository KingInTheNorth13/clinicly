
import { Clock } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { ScrollArea } from "@/components/ui/scroll-area"

interface TimePickerProps {
  time?: string
  onTimeChange?: (time: string) => void
  placeholder?: string
  disabled?: boolean
  className?: string
}

export function TimePicker({
  time,
  onTimeChange,
  placeholder = "Select time",
  disabled = false,
  className
}: TimePickerProps) {
  const generateTimeSlots = () => {
    const slots = [];
    for (let hour = 8; hour <= 18; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        const timeString = `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}`;
        const displayTime = new Date(`2000-01-01T${timeString}`).toLocaleTimeString('en-US', {
          hour: 'numeric',
          minute: '2-digit',
          hour12: true
        });
        slots.push({ value: timeString, display: displayTime });
      }
    }
    return slots;
  };

  const timeSlots = generateTimeSlots();
  const displayTime = time ? 
    new Date(`2000-01-01T${time}`).toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    }) : null;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant={"outline"}
          className={cn(
            "medical-input justify-start text-left font-normal",
            !time && "text-muted-foreground",
            className
          )}
          disabled={disabled}
        >
          <Clock className="mr-2 h-4 w-4 text-teal-500" />
          {displayTime || <span>{placeholder}</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="medical-card w-64 p-0" align="start">
        <ScrollArea className="h-64">
          <div className="p-2">
            {timeSlots.map((slot) => (
              <Button
                key={slot.value}
                variant="ghost"
                className={cn(
                  "w-full justify-start font-normal hover:bg-teal-50 hover:text-teal-700 dark:hover:bg-teal-900/20",
                  time === slot.value && "bg-teal-100 text-teal-700 dark:bg-teal-900/40 dark:text-teal-300"
                )}
                onClick={() => onTimeChange?.(slot.value)}
              >
                {slot.display}
              </Button>
            ))}
          </div>
        </ScrollArea>
      </PopoverContent>
    </Popover>
  )
}