import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Shield, ArrowLeft, Loader2 } from 'lucide-react';
import type { User } from '@/types';

interface ProtectedRouteProps {
  children: ReactNode;
  requiredRole?: User['role'] | User['role'][];
  fallbackPath?: string;
}

export function ProtectedRoute({ 
  children, 
  requiredRole, 
  fallbackPath = '/dashboard' 
}: ProtectedRouteProps) {
  const { isAuthenticated, user, loading } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking authentication
  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-white to-cyan-50">
        <div className="flex flex-col items-center space-y-4">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <p className="text-sm text-muted-foreground animate-pulse">Verifying access...</p>
        </div>
      </div>
    );
  }

  // Check authentication from both React state and localStorage as fallback
  const token = localStorage.getItem('authToken');
  const storedUser = localStorage.getItem('user');
  const hasValidAuth = (isAuthenticated && user) || (token && storedUser);

  // Redirect to login if not authenticated
  if (!hasValidAuth) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Use the user from React state, or parse from localStorage as fallback
  const currentUser = user || (storedUser ? JSON.parse(storedUser) : null);

  // Check role-based access if required
  if (requiredRole && currentUser) {
    const allowedRoles = Array.isArray(requiredRole) ? requiredRole : [requiredRole];
    const hasAccess = allowedRoles.includes(currentUser.role);
    
    if (!hasAccess) {
      return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-red-50 via-white to-orange-50 p-4">
          <Card className="w-full max-w-md shadow-lg">
            <CardHeader className="text-center">
              <div className="mx-auto w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mb-4">
                <Shield className="w-6 h-6 text-red-600" />
              </div>
              <CardTitle className="text-xl text-red-900">Access Restricted</CardTitle>
              <CardDescription className="text-red-700">
                You don't have the required permissions to access this page.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                <p className="text-sm text-red-800">
                  <strong>Your role:</strong> {currentUser.role}
                </p>
                <p className="text-sm text-red-800">
                  <strong>Required role(s):</strong> {allowedRoles.join(', ')}
                </p>
              </div>
              
              <div className="flex flex-col space-y-2">
                <Button 
                  onClick={() => window.history.back()} 
                  variant="outline"
                  className="w-full"
                >
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Go Back
                </Button>
                <Button 
                  onClick={() => window.location.href = fallbackPath}
                  className="w-full"
                >
                  Go to Dashboard
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      );
    }
  }

  return <>{children}</>;
}