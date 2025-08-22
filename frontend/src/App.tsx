import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { AppLayout } from './components/layout';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { LoginPage, DashboardPage, AppointmentsPage, PatientsPage } from './pages';
import { Toaster } from '@/components/ui/sonner';
import './index.css';
import './App.css';

// Placeholder components for admin routes
const AdminDashboard = () => (
  <div className="p-6">
    <h1 className="text-2xl font-bold mb-4">Admin Dashboard</h1>
    <p className="text-muted-foreground">Administrative functions and clinic-wide overview.</p>
  </div>
);

const ReportsPage = () => (
  <div className="p-6">
    <h1 className="text-2xl font-bold mb-4">Reports & Analytics</h1>
    <p className="text-muted-foreground">View clinic statistics and generate reports.</p>
  </div>
);

const SchedulePage = () => (
  <div className="p-6">
    <h1 className="text-2xl font-bold mb-4">My Schedule</h1>
    <p className="text-muted-foreground">Your daily appointment schedule.</p>
  </div>
);

const SettingsPage = () => (
  <div className="p-6">
    <h1 className="text-2xl font-bold mb-4">Settings</h1>
    <p className="text-muted-foreground">Configure your application preferences.</p>
  </div>
);

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          
          {/* Protected routes with shared layout */}
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            {/* Default redirect */}
            <Route index element={<Navigate to="/dashboard" replace />} />
            
            {/* Common routes for all authenticated users */}
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="appointments" element={<AppointmentsPage />} />
            <Route path="patients" element={<PatientsPage />} />
            <Route path="settings" element={<SettingsPage />} />
            
            {/* Doctor-only routes */}
            <Route 
              path="schedule" 
              element={
                <ProtectedRoute requiredRole="Doctor">
                  <SchedulePage />
                </ProtectedRoute>
              } 
            />
            
            {/* Admin-only routes */}
            <Route 
              path="admin" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <AdminDashboard />
                </ProtectedRoute>
              } 
            />
            <Route 
              path="reports" 
              element={
                <ProtectedRoute requiredRole="Admin">
                  <ReportsPage />
                </ProtectedRoute>
              } 
            />
          </Route>
          
          {/* Catch all route - redirect to dashboard if authenticated, login if not */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
        
        {/* Toast notifications */}
        <Toaster />
      </Router>
    </AuthProvider>
  );
}

export default App;
