import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { useAuth } from '@/hooks/useAuth';
import { Stethoscope, Mail, Lock, Loader2 } from 'lucide-react';
import type { LoginRequest } from '@/types';

const loginSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(1, 'Password is required'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login, isAuthenticated, loading } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      const from = location.state?.from?.pathname || '/dashboard';
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, location]);

  const onSubmit = async (data: LoginFormData) => {
    try {
      setIsSubmitting(true);
      await login(data as LoginRequest);
      
      // Navigation will be handled by the useEffect above
    } catch (error) {
      // Error handling is done in the AuthContext
      console.error('Login error:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #f8fafc 0%, #e0f2fe 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{
            width: '64px',
            height: '64px',
            backgroundColor: '#2563eb',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 16px',
            boxShadow: '0 10px 25px rgba(37, 99, 235, 0.3)'
          }}>
            <Stethoscope size={32} color="white" />
          </div>
          <div style={{
            width: '32px',
            height: '32px',
            border: '2px solid #2563eb',
            borderTop: '2px solid transparent',
            borderRadius: '50%',
            margin: '0 auto 8px',
            animation: 'spin 1s linear infinite'
          }}></div>
          <p style={{ fontSize: '14px', color: '#6b7280' }}>Loading your session...</p>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #f8fafc 0%, #e0f2fe 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px'
    }}>
      <div style={{
        width: '100%',
        maxWidth: '400px',
        margin: '0 auto'
      }}>
        {/* Logo and branding */}
        <div style={{ textAlign: 'center', marginBottom: '40px' }}>
          <div style={{
            width: '64px',
            height: '64px',
            backgroundColor: '#2563eb',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 10px 25px rgba(37, 99, 235, 0.3)'
          }}>
            <Stethoscope size={32} color="white" />
          </div>
          <h1 style={{
            fontSize: '32px',
            fontWeight: 'bold',
            color: '#111827',
            marginBottom: '8px',
            margin: '0 0 8px 0'
          }}>
            Clinic Manager
          </h1>
          <p style={{
            fontSize: '14px',
            color: '#6b7280',
            margin: '0'
          }}>
            Professional appointment management system
          </p>
        </div>

        {/* Login Card */}
        <div style={{
          backgroundColor: 'white',
          borderRadius: '12px',
          boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
          padding: '40px'
        }}>
          {/* Card Header */}
          <div style={{ textAlign: 'center', marginBottom: '32px' }}>
            <h2 style={{
              fontSize: '24px',
              fontWeight: '600',
              color: '#111827',
              marginBottom: '8px',
              margin: '0 0 8px 0'
            }}>
              Welcome Back
            </h2>
            <p style={{ 
              color: '#6b7280', 
              fontSize: '14px',
              margin: '0'
            }}>
              Sign in to your account to continue
            </p>
          </div>

          {/* Form */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} style={{ marginBottom: '24px' }}>
              <div style={{ marginBottom: '20px' }}>
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '14px',
                        fontWeight: '500',
                        color: '#374151',
                        marginBottom: '6px'
                      }}>
                        Email address
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '12px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1
                          }}>
                            <Mail size={20} color="#9ca3af" />
                          </div>
                          <Input
                            type="email"
                            autoComplete="email"
                            placeholder="doctor@clinic.com"
                            style={{
                              width: '100%',
                              paddingLeft: '44px',
                              paddingRight: '12px',
                              paddingTop: '12px',
                              paddingBottom: '12px',
                              border: '1px solid #d1d5db',
                              borderRadius: '8px',
                              fontSize: '14px',
                              outline: 'none',
                              boxSizing: 'border-box'
                            }}
                            {...field}
                          />
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '12px', color: '#dc2626', marginTop: '4px' }} />
                    </FormItem>
                  )}
                />
              </div>

              <div style={{ marginBottom: '24px' }}>
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '14px',
                        fontWeight: '500',
                        color: '#374151',
                        marginBottom: '6px'
                      }}>
                        Password
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '12px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1
                          }}>
                            <Lock size={20} color="#9ca3af" />
                          </div>
                          <Input
                            type="password"
                            autoComplete="current-password"
                            placeholder="Enter your password"
                            style={{
                              width: '100%',
                              paddingLeft: '44px',
                              paddingRight: '12px',
                              paddingTop: '12px',
                              paddingBottom: '12px',
                              border: '1px solid #d1d5db',
                              borderRadius: '8px',
                              fontSize: '14px',
                              outline: 'none',
                              boxSizing: 'border-box'
                            }}
                            {...field}
                          />
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '12px', color: '#dc2626', marginTop: '4px' }} />
                    </FormItem>
                  )}
                />
              </div>

              <Button
                type="submit"
                disabled={isSubmitting}
                style={{
                  width: '100%',
                  backgroundColor: isSubmitting ? '#9ca3af' : '#2563eb',
                  color: 'white',
                  border: 'none',
                  borderRadius: '8px',
                  padding: '12px 16px',
                  fontSize: '14px',
                  fontWeight: '500',
                  cursor: isSubmitting ? 'not-allowed' : 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '8px',
                  boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
                  transition: 'all 0.2s'
                }}
              >
                {isSubmitting ? (
                  <>
                    <Loader2 size={16} style={{ animation: 'spin 1s linear infinite' }} />
                    Signing in...
                  </>
                ) : (
                  'Sign in'
                )}
              </Button>
            </form>
          </Form>

          {/* Demo credentials */}
          <div>
            <div style={{ 
              position: 'relative', 
              marginBottom: '16px',
              textAlign: 'center'
            }}>
              <div style={{
                position: 'absolute',
                top: '50%',
                left: 0,
                right: 0,
                height: '1px',
                backgroundColor: '#d1d5db'
              }}></div>
              <span style={{
                backgroundColor: 'white',
                padding: '0 12px',
                fontSize: '14px',
                color: '#6b7280'
              }}>
                Demo Credentials
              </span>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              <div style={{
                backgroundColor: '#eff6ff',
                border: '1px solid #bfdbfe',
                borderRadius: '8px',
                padding: '16px'
              }}>
                <div style={{ fontSize: '14px', color: '#1e40af' }}>
                  <p style={{ fontWeight: '600', margin: '0 0 8px 0' }}>Doctor Account</p>
                  <p style={{ margin: '0 0 4px 0', fontSize: '12px' }}>
                    Email: <code style={{ backgroundColor: '#dbeafe', padding: '2px 4px', borderRadius: '4px' }}>doctor@clinic.com</code>
                  </p>
                  <p style={{ margin: '0', fontSize: '12px' }}>
                    Password: <code style={{ backgroundColor: '#dbeafe', padding: '2px 4px', borderRadius: '4px' }}>password</code>
                  </p>
                </div>
              </div>
              <div style={{
                backgroundColor: '#faf5ff',
                border: '1px solid #d8b4fe',
                borderRadius: '8px',
                padding: '16px'
              }}>
                <div style={{ fontSize: '14px', color: '#7c3aed' }}>
                  <p style={{ fontWeight: '600', margin: '0 0 8px 0' }}>Admin Account</p>
                  <p style={{ margin: '0 0 4px 0', fontSize: '12px' }}>
                    Email: <code style={{ backgroundColor: '#f3e8ff', padding: '2px 4px', borderRadius: '4px' }}>admin@clinic.com</code>
                  </p>
                  <p style={{ margin: '0', fontSize: '12px' }}>
                    Password: <code style={{ backgroundColor: '#f3e8ff', padding: '2px 4px', borderRadius: '4px' }}>password</code>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div style={{ textAlign: 'center', marginTop: '32px' }}>
          <p style={{ fontSize: '12px', color: '#6b7280', margin: '0' }}>
            Secure • Reliable • Professional Healthcare Management
          </p>
        </div>
      </div>

      {/* CSS for animations */}
      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
}