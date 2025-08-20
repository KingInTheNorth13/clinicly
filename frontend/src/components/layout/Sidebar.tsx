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
    <Card className="w-64 min-h-screen rounded-none border-r bg-gray-50/50">
      <div className="p-4">
        <div className="mb-6">
          <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">
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
                    ? 'bg-blue-600 text-white shadow-md'
                    : 'text-gray-700 hover:bg-white hover:text-gray-900 hover:shadow-sm'
                )
              }
            >
              {({ isActive }) => (
                <>
                  <div className="flex items-center space-x-3">
                    <item.icon 
                      className={cn(
                        "h-4 w-4 transition-colors",
                        isActive ? "text-white" : "text-gray-500 group-hover:text-gray-700"
                      )} 
                    />
                    <div className="flex flex-col">
                      <span className="text-sm">{item.name}</span>
                      {item.description && (
                        <span className={cn(
                          "text-xs transition-colors",
                          isActive ? "text-blue-100" : "text-gray-500 group-hover:text-gray-600"
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
                          ? "bg-blue-500 text-white border-blue-400" 
                          : "bg-purple-100 text-purple-700 border-purple-200"
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
        <div className="mt-8 p-3 bg-white rounded-lg border border-gray-200">
          <div className="flex items-center space-x-2">
            <div className="w-2 h-2 bg-green-500 rounded-full"></div>
            <span className="text-xs text-gray-600">Logged in as</span>
          </div>
          <p className="text-sm font-medium text-gray-900 mt-1">
            {user?.role}
          </p>
        </div>
      </div>
    </Card>
  );
}