import { NavLink } from 'react-router-dom';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { 
  Calendar, 
  Users, 
  LayoutDashboard, 
  Settings, 
  UserCog,
  BarChart3,
  Clock
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useAuth } from '@/hooks/useAuth';
import type { User } from '@/types';

interface NavigationItem {
  name: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  roles?: User['role'][];
  badge?: string;
  description?: string;
}

const navigation: NavigationItem[] = [
  { 
    name: 'Dashboard', 
    href: '/dashboard', 
    icon: LayoutDashboard,
    description: 'Overview and quick actions'
  },
  { 
    name: 'Appointments', 
    href: '/appointments', 
    icon: Calendar,
    description: 'Manage your appointments'
  },
  { 
    name: 'Patients', 
    href: '/patients', 
    icon: Users,
    description: 'Patient records and information'
  },
  { 
    name: 'Admin Panel', 
    href: '/admin', 
    icon: UserCog, 
    roles: ['Admin'],
    badge: 'Admin',
    description: 'Administrative functions'
  },
  { 
    name: 'Reports', 
    href: '/reports', 
    icon: BarChart3, 
    roles: ['Admin'],
    description: 'Analytics and reports'
  },
  { 
    name: 'Schedule', 
    href: '/schedule', 
    icon: Clock,
    roles: ['Doctor'],
    description: 'Your daily schedule'
  },
  { 
    name: 'Settings', 
    href: '/settings', 
    icon: Settings,
    description: 'Application settings'
  },
];

export function Sidebar() {
  const { user } = useAuth();

  const filteredNavigation = navigation.filter(item => {
    if (!item.roles) return true;
    return user?.role && item.roles.includes(user.role);
  });

  return (
    <Card className="w-64 min-h-screen rounded-none border-r bg-muted/30">
      <div className="p-4">
        <div className="mb-6">
          <h2 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-3">
            Navigation
          </h2>
        </div>
        
        <nav className="space-y-1">
          {filteredNavigation.map((item) => (
            <NavLink
              key={item.name}
              to={item.href}
              className={({ isActive }) =>
                cn(
                  'group flex items-center justify-between px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-200',
                  isActive
                    ? 'bg-primary text-primary-foreground shadow-md'
                    : 'text-foreground hover:bg-card hover:text-foreground hover:shadow-sm'
                )
              }
            >
              {({ isActive }) => (
                <>
                  <div className="flex items-center space-x-3">
                    <item.icon 
                      className={cn(
                        "h-4 w-4 transition-colors",
                        isActive ? "text-primary-foreground" : "text-muted-foreground group-hover:text-foreground"
                      )} 
                    />
                    <div className="flex flex-col">
                      <span className="text-sm">{item.name}</span>
                      {item.description && (
                        <span className={cn(
                          "text-xs transition-colors",
                          isActive ? "text-primary-foreground/80" : "text-muted-foreground group-hover:text-foreground/80"
                        )}>
                          {item.description}
                        </span>
                      )}
                    </div>
                  </div>
                  
                  {item.badge && (
                    <Badge 
                      variant="secondary" 
                      className={cn(
                        "text-xs px-2 py-0.5",
                        isActive 
                          ? "bg-primary/20 text-primary-foreground border-primary/30" 
                          : "bg-primary/10 text-primary border-primary/20"
                      )}
                    >
                      {item.badge}
                    </Badge>
                  )}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        {/* User Role Info */}
        <div className="mt-8 p-3 bg-card rounded-lg border border-border">
          <div className="flex items-center space-x-2">
            <div className="w-2 h-2 bg-primary rounded-full"></div>
            <span className="text-xs text-muted-foreground">Logged in as</span>
          </div>
          <p className="text-sm font-medium text-foreground mt-1">
            {user?.role}
          </p>
        </div>
      </div>
    </Card>
  );
}