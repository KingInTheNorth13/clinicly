import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { useAuth } from '@/hooks/useAuth';
import { Stethoscope, Mail, Lock, Loader2, Eye, EyeOff, Sparkles } from 'lucide-react';
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
  const [showPassword, setShowPassword] = useState(false);
  const [isVisible, setIsVisible] = useState(false);
  const [focusedField, setFocusedField] = useState<string | null>(null);

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  // Animation trigger
  useEffect(() => {
    const timer = setTimeout(() => setIsVisible(true), 100);
    return () => clearTimeout(timer);
  }, []);

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
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        position: 'relative',
        overflow: 'hidden'
      }}>
        {/* Animated background particles */}
        <div style={{
          position: 'absolute',
          width: '100%',
          height: '100%',
          background: 'radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3) 0%, transparent 50%), radial-gradient(circle at 80% 20%, rgba(255, 255, 255, 0.15) 0%, transparent 50%), radial-gradient(circle at 40% 40%, rgba(120, 119, 198, 0.2) 0%, transparent 50%)',
          animation: 'float 6s ease-in-out infinite'
        }}></div>
        
        <div style={{ 
          textAlign: 'center', 
          zIndex: 10,
          transform: isVisible ? 'translateY(0) scale(1)' : 'translateY(20px) scale(0.9)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1)'
        }}>
          <div style={{
            width: '80px',
            height: '80px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 20px 40px rgba(102, 126, 234, 0.4), 0 0 0 10px rgba(255, 255, 255, 0.1)',
            animation: 'pulse 2s ease-in-out infinite',
            border: '3px solid rgba(255, 255, 255, 0.2)'
          }}>
            <Stethoscope size={40} color="white" />
          </div>
          <div style={{
            width: '40px',
            height: '40px',
            border: '3px solid rgba(255, 255, 255, 0.3)',
            borderTop: '3px solid white',
            borderRadius: '50%',
            margin: '0 auto 16px',
            animation: 'spin 1s linear infinite'
          }}></div>
          <p style={{ 
            fontSize: '16px', 
            color: 'white', 
            fontWeight: '500',
            textShadow: '0 2px 4px rgba(0, 0, 0, 0.3)'
          }}>
            Loading your session...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Animated background elements */}
      <div style={{
        position: 'absolute',
        width: '100%',
        height: '100%',
        background: 'radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3) 0%, transparent 50%), radial-gradient(circle at 80% 20%, rgba(255, 255, 255, 0.15) 0%, transparent 50%), radial-gradient(circle at 40% 40%, rgba(120, 119, 198, 0.2) 0%, transparent 50%)',
        animation: 'float 6s ease-in-out infinite'
      }}></div>
      
      {/* Floating particles */}
      <div style={{
        position: 'absolute',
        width: '6px',
        height: '6px',
        backgroundColor: 'rgba(255, 255, 255, 0.6)',
        borderRadius: '50%',
        top: '20%',
        left: '10%',
        animation: 'float 4s ease-in-out infinite'
      }}></div>
      <div style={{
        position: 'absolute',
        width: '4px',
        height: '4px',
        backgroundColor: 'rgba(255, 255, 255, 0.4)',
        borderRadius: '50%',
        top: '60%',
        right: '15%',
        animation: 'float 5s ease-in-out infinite 1s'
      }}></div>
      <div style={{
        position: 'absolute',
        width: '8px',
        height: '8px',
        backgroundColor: 'rgba(255, 255, 255, 0.3)',
        borderRadius: '50%',
        bottom: '30%',
        left: '20%',
        animation: 'float 6s ease-in-out infinite 2s'
      }}></div>

      <div style={{
        width: '100%',
        maxWidth: '420px',
        margin: '0 auto',
        zIndex: 10,
        transform: isVisible ? 'translateY(0) scale(1)' : 'translateY(30px) scale(0.95)',
        opacity: isVisible ? 1 : 0,
        transition: 'all 1s cubic-bezier(0.16, 1, 0.3, 1)'
      }}>
        {/* Logo and branding */}
        <div style={{ 
          textAlign: 'center', 
          marginBottom: '40px',
          transform: isVisible ? 'translateY(0)' : 'translateY(-20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.2s'
        }}>
          <div style={{
            width: '80px',
            height: '80px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 20px 40px rgba(102, 126, 234, 0.4), 0 0 0 10px rgba(255, 255, 255, 0.1)',
            animation: 'pulse 3s ease-in-out infinite',
            border: '3px solid rgba(255, 255, 255, 0.2)',
            position: 'relative'
          }}>
            <Stethoscope size={40} color="white" />
            <div style={{
              position: 'absolute',
              top: '-5px',
              right: '-5px',
              width: '20px',
              height: '20px',
              background: 'linear-gradient(45deg, #ffd700, #ffed4e)',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              animation: 'sparkle 2s ease-in-out infinite'
            }}>
              <Sparkles size={12} color="white" />
            </div>
          </div>
          <h1 style={{
            fontSize: 'clamp(28px, 5vw, 36px)',
            fontWeight: '700',
            color: 'white',
            marginBottom: '8px',
            margin: '0 0 8px 0',
            textShadow: '0 4px 8px rgba(0, 0, 0, 0.3)',
            letterSpacing: '-0.5px'
          }}>
            Clinic Manager
          </h1>
          <p style={{
            fontSize: '16px',
            color: 'rgba(255, 255, 255, 0.9)',
            margin: '0',
            textShadow: '0 2px 4px rgba(0, 0, 0, 0.3)'
          }}>
            Professional appointment management system
          </p>
        </div>

        {/* Login Card */}
        <div style={{
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderRadius: '20px',
          boxShadow: '0 32px 64px rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(255, 255, 255, 0.1)',
          padding: '40px',
          backdropFilter: 'blur(20px)',
          border: '1px solid rgba(255, 255, 255, 0.2)',
          transform: isVisible ? 'translateY(0)' : 'translateY(20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.4s'
        }}>
          {/* Card Header */}
          <div style={{ textAlign: 'center', marginBottom: '32px' }}>
            <h2 style={{
              fontSize: '28px',
              fontWeight: '700',
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
              marginBottom: '8px',
              margin: '0 0 8px 0'
            }}>
              Welcome Back
            </h2>
            <p style={{ 
              color: '#6b7280', 
              fontSize: '16px',
              margin: '0'
            }}>
              Sign in to your account to continue
            </p>
          </div>

          {/* Form */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} style={{ marginBottom: '24px' }}>
              <div style={{ marginBottom: '24px' }}>
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '14px',
                        fontWeight: '600',
                        color: '#374151',
                        marginBottom: '8px'
                      }}>
                        Email address
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '16px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1,
                            color: focusedField === 'email' ? '#667eea' : '#9ca3af',
                            transition: 'color 0.3s ease'
                          }}>
                            <Mail size={20} />
                          </div>
                          <Input
                            type="email"
                            autoComplete="email"
                            placeholder="doctor@clinic.com"
                            style={{
                              width: '100%',
                              paddingLeft: '48px',
                              paddingRight: '16px',
                              paddingTop: '14px',
                              paddingBottom: '14px',
                              border: `2px solid ${focusedField === 'email' ? '#667eea' : '#e5e7eb'}`,
                              borderRadius: '12px',
                              fontSize: '16px',
                              outline: 'none',
                              boxSizing: 'border-box',
                              transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                              boxShadow: focusedField === 'email' ? '0 0 0 4px rgba(102, 126, 234, 0.1)' : 'none',
                              transform: focusedField === 'email' ? 'translateY(-2px)' : 'translateY(0)'
                            }}
                            {...field}
                            onFocus={() => setFocusedField('email')}
                            onBlur={() => setFocusedField(null)}
                          />
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '12px', color: '#dc2626', marginTop: '6px' }} />
                    </FormItem>
                  )}
                />
              </div>

              <div style={{ marginBottom: '32px' }}>
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '14px',
                        fontWeight: '600',
                        color: '#374151',
                        marginBottom: '8px'
                      }}>
                        Password
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '16px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1,
                            color: focusedField === 'password' ? '#667eea' : '#9ca3af',
                            transition: 'color 0.3s ease'
                          }}>
                            <Lock size={20} />
                          </div>
                          <Input
                            type={showPassword ? 'text' : 'password'}
                            autoComplete="current-password"
                            placeholder="Enter your password"
                            style={{
                              width: '100%',
                              paddingLeft: '48px',
                              paddingRight: '48px',
                              paddingTop: '14px',
                              paddingBottom: '14px',
                              border: `2px solid ${focusedField === 'password' ? '#667eea' : '#e5e7eb'}`,
                              borderRadius: '12px',
                              fontSize: '16px',
                              outline: 'none',
                              boxSizing: 'border-box',
                              transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                              boxShadow: focusedField === 'password' ? '0 0 0 4px rgba(102, 126, 234, 0.1)' : 'none',
                              transform: focusedField === 'password' ? 'translateY(-2px)' : 'translateY(0)'
                            }}
                            {...field}
                            onFocus={() => setFocusedField('password')}
                            onBlur={() => setFocusedField(null)}
                          />
                          <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                            style={{
                              position: 'absolute',
                              right: '16px',
                              top: '50%',
                              transform: 'translateY(-50%)',
                              background: 'none',
                              border: 'none',
                              cursor: 'pointer',
                              color: '#9ca3af',
                              padding: '4px',
                              borderRadius: '4px',
                              transition: 'all 0.2s ease'
                            }}
                          >
                            {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                          </button>
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '12px', color: '#dc2626', marginTop: '6px' }} />
                    </FormItem>
                  )}
                />
              </div>

              <Button
                type="submit"
                disabled={isSubmitting}
                style={{
                  width: '100%',
                  background: isSubmitting ? '#9ca3af' : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  border: 'none',
                  borderRadius: '12px',
                  padding: '16px 24px',
                  fontSize: '16px',
                  fontWeight: '600',
                  cursor: isSubmitting ? 'not-allowed' : 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '8px',
                  boxShadow: isSubmitting ? 'none' : '0 8px 16px rgba(102, 126, 234, 0.3)',
                  transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                  transform: isSubmitting ? 'scale(0.98)' : 'scale(1)',
                  position: 'relative',
                  overflow: 'hidden'
                }}
                onMouseEnter={(e) => {
                  if (!isSubmitting) {
                    e.currentTarget.style.transform = 'translateY(-2px) scale(1.02)';
                    e.currentTarget.style.boxShadow = '0 12px 24px rgba(102, 126, 234, 0.4)';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!isSubmitting) {
                    e.currentTarget.style.transform = 'translateY(0) scale(1)';
                    e.currentTarget.style.boxShadow = '0 8px 16px rgba(102, 126, 234, 0.3)';
                  }
                }}
              >
                {isSubmitting ? (
                  <>
                    <Loader2 size={20} style={{ animation: 'spin 1s linear infinite' }} />
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
              marginBottom: '20px',
              textAlign: 'center'
            }}>
              <div style={{
                position: 'absolute',
                top: '50%',
                left: 0,
                right: 0,
                height: '1px',
                background: 'linear-gradient(90deg, transparent, #e5e7eb, transparent)'
              }}></div>
              <span style={{
                backgroundColor: 'rgba(255, 255, 255, 0.95)',
                padding: '0 16px',
                fontSize: '14px',
                color: '#6b7280',
                fontWeight: '500'
              }}>
                Demo Credentials
              </span>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              <div style={{
                background: 'linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%)',
                border: '1px solid #bfdbfe',
                borderRadius: '12px',
                padding: '16px',
                transition: 'all 0.3s ease',
                cursor: 'pointer'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-2px)';
                e.currentTarget.style.boxShadow = '0 8px 16px rgba(59, 130, 246, 0.15)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = 'none';
              }}>
                <div style={{ fontSize: '14px', color: '#1e40af' }}>
                  <p style={{ fontWeight: '600', margin: '0 0 8px 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{
                      width: '8px',
                      height: '8px',
                      backgroundColor: '#3b82f6',
                      borderRadius: '50%'
                    }}></span>
                    Doctor Account
                  </p>
                  <p style={{ margin: '0 0 4px 0', fontSize: '12px' }}>
                    Email: <code style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>doctor@clinic.com</code>
                  </p>
                  <p style={{ margin: '0', fontSize: '12px' }}>
                    Password: <code style={{ backgroundColor: 'rgba(59, 130, 246, 0.1)', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>password</code>
                  </p>
                </div>
              </div>
              <div style={{
                background: 'linear-gradient(135deg, #faf5ff 0%, #f3e8ff 100%)',
                border: '1px solid #d8b4fe',
                borderRadius: '12px',
                padding: '16px',
                transition: 'all 0.3s ease',
                cursor: 'pointer'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateY(-2px)';
                e.currentTarget.style.boxShadow = '0 8px 16px rgba(147, 51, 234, 0.15)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.boxShadow = 'none';
              }}>
                <div style={{ fontSize: '14px', color: '#7c3aed' }}>
                  <p style={{ fontWeight: '600', margin: '0 0 8px 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{
                      width: '8px',
                      height: '8px',
                      backgroundColor: '#8b5cf6',
                      borderRadius: '50%'
                    }}></span>
                    Admin Account
                  </p>
                  <p style={{ margin: '0 0 4px 0', fontSize: '12px' }}>
                    Email: <code style={{ backgroundColor: 'rgba(147, 51, 234, 0.1)', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>admin@clinic.com</code>
                  </p>
                  <p style={{ margin: '0', fontSize: '12px' }}>
                    Password: <code style={{ backgroundColor: 'rgba(147, 51, 234, 0.1)', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>password</code>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div style={{ 
          textAlign: 'center', 
          marginTop: '32px',
          transform: isVisible ? 'translateY(0)' : 'translateY(20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.6s'
        }}>
          <p style={{ 
            fontSize: '14px', 
            color: 'rgba(255, 255, 255, 0.8)', 
            margin: '0',
            textShadow: '0 2px 4px rgba(0, 0, 0, 0.3)'
          }}>
            Secure • Reliable • Professional Healthcare Management
          </p>
        </div>
      </div>

      {/* Enhanced CSS for animations */}
      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
        
        @keyframes float {
          0%, 100% { transform: translateY(0px) rotate(0deg); }
          33% { transform: translateY(-10px) rotate(1deg); }
          66% { transform: translateY(5px) rotate(-1deg); }
        }
        
        @keyframes pulse {
          0%, 100% { 
            transform: scale(1);
            box-shadow: 0 20px 40px rgba(102, 126, 234, 0.4), 0 0 0 10px rgba(255, 255, 255, 0.1);
          }
          50% { 
            transform: scale(1.05);
            box-shadow: 0 25px 50px rgba(102, 126, 234, 0.6), 0 0 0 15px rgba(255, 255, 255, 0.15);
          }
        }
        
        @keyframes sparkle {
          0%, 100% { 
            transform: scale(1) rotate(0deg);
            opacity: 1;
          }
          50% { 
            transform: scale(1.2) rotate(180deg);
            opacity: 0.8;
          }
        }
        
        @media (max-width: 480px) {
          .login-container {
            padding: 16px !important;
          }
          .login-card {
            padding: 24px !important;
          }
        }
      `}</style>
    </div>
  );
}