import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form';
import { useAuth } from '@/hooks/useAuth';
import { Stethoscope, Mail, Lock, Loader2, Eye, EyeOff, Calendar, Heart, Shield, HelpCircle } from 'lucide-react';
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
      
      // Force navigation after successful login
      const from = location.state?.from?.pathname || '/dashboard';
      navigate(from, { replace: true });
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
        height: '100vh',
        width: '100vw',
        background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #334155 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        margin: 0,
        overflow: 'hidden',
        fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        boxSizing: 'border-box'
      }}>
        {/* Medical-themed background pattern */}
        <div style={{
          position: 'absolute',
          width: '100%',
          height: '100%',
          backgroundImage: `
            radial-gradient(circle at 25% 25%, rgba(20, 184, 166, 0.1) 0%, transparent 50%),
            radial-gradient(circle at 75% 75%, rgba(59, 130, 246, 0.1) 0%, transparent 50%),
            radial-gradient(circle at 50% 50%, rgba(16, 185, 129, 0.05) 0%, transparent 50%)
          `,
          animation: 'medicalFloat 8s ease-in-out infinite'
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
            background: 'linear-gradient(135deg, #14b8a6 0%, #0891b2 100%)',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 20px 40px rgba(20, 184, 166, 0.3), 0 0 0 10px rgba(20, 184, 166, 0.1)',
            animation: 'medicalPulse 2s ease-in-out infinite',
            border: '2px solid rgba(20, 184, 166, 0.3)'
          }}>
            <Stethoscope size={40} color="white" />
          </div>
          <div style={{
            width: '40px',
            height: '40px',
            border: '3px solid rgba(20, 184, 166, 0.3)',
            borderTop: '3px solid #14b8a6',
            borderRadius: '50%',
            margin: '0 auto 16px',
            animation: 'spin 1s linear infinite'
          }}></div>
          <p style={{ 
            fontSize: '16px', 
            color: '#e2e8f0', 
            fontWeight: '500'
          }}>
            Loading your session...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="login-container" style={{
      minHeight: '100vh',
      height: '100vh',
      width: '100vw',
      background: 'linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #334155 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px',
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      margin: 0,
      overflow: 'hidden',
      fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      boxSizing: 'border-box'
    }}>
      {/* Medical-themed background elements */}
      <div style={{
        position: 'absolute',
        width: '100%',
        height: '100%',
        backgroundImage: `
          radial-gradient(circle at 25% 25%, rgba(20, 184, 166, 0.1) 0%, transparent 50%),
          radial-gradient(circle at 75% 75%, rgba(59, 130, 246, 0.1) 0%, transparent 50%),
          radial-gradient(circle at 50% 50%, rgba(16, 185, 129, 0.05) 0%, transparent 50%)
        `,
        animation: 'medicalFloat 8s ease-in-out infinite'
      }}></div>
      
      {/* Floating medical icons */}
      <div style={{
        position: 'absolute',
        top: '15%',
        left: '10%',
        color: 'rgba(20, 184, 166, 0.2)',
        animation: 'medicalFloat 6s ease-in-out infinite'
      }}>
        <Heart size={24} />
      </div>
      <div style={{
        position: 'absolute',
        top: '70%',
        right: '15%',
        color: 'rgba(59, 130, 246, 0.2)',
        animation: 'medicalFloat 8s ease-in-out infinite 2s'
      }}>
        <Calendar size={20} />
      </div>
      <div style={{
        position: 'absolute',
        bottom: '25%',
        left: '20%',
        color: 'rgba(16, 185, 129, 0.2)',
        animation: 'medicalFloat 7s ease-in-out infinite 1s'
      }}>
        <Shield size={28} />
      </div>

      <div style={{
        width: '100%',
        maxWidth: '520px',
        margin: '0 auto',
        zIndex: 10,
        transform: isVisible ? 'translateY(0) scale(1)' : 'translateY(30px) scale(0.95)',
        opacity: isVisible ? 1 : 0,
        transition: 'all 1s cubic-bezier(0.16, 1, 0.3, 1)',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center'
      }}>
        {/* Logo and branding */}
        <div style={{ 
          textAlign: 'center', 
          marginBottom: '40px',
          transform: isVisible ? 'translateY(0)' : 'translateY(-20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.2s',
          width: '100%'
        }}>
          <div style={{
            width: '90px',
            height: '90px',
            background: 'linear-gradient(135deg, #14b8a6 0%, #0891b2 100%)',
            borderRadius: '50%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 25px 50px rgba(20, 184, 166, 0.3), 0 0 0 15px rgba(20, 184, 166, 0.1)',
            animation: 'medicalPulse 3s ease-in-out infinite',
            border: '3px solid rgba(20, 184, 166, 0.2)',
            position: 'relative'
          }}>
            <Stethoscope size={45} color="white" />
            <div style={{
              position: 'absolute',
              top: '-8px',
              right: '-8px',
              width: '24px',
              height: '24px',
              background: 'linear-gradient(45deg, #10b981, #14b8a6)',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              animation: 'medicalSparkle 2s ease-in-out infinite',
              border: '2px solid #0f172a'
            }}>
              <Heart size={12} color="white" />
            </div>
          </div>
          <h1 style={{
            fontSize: 'clamp(32px, 5vw, 42px)',
            fontWeight: '800',
            color: '#f8fafc',
            marginBottom: '12px',
            margin: '0 0 12px 0',
            letterSpacing: '-1px',
            textShadow: '0 4px 8px rgba(0, 0, 0, 0.3)'
          }}>
           Clinicly
          </h1>
          <p style={{
            fontSize: '18px',
            color: '#94a3b8',
            margin: '0',
            fontWeight: '500'
          }}>
            Advanced Clinic Management System
          </p>
        </div>

        {/* Login Card */}
        <div className="login-card" style={{
          backgroundColor: 'rgba(30, 41, 59, 0.95)',
          borderRadius: '28px',
          boxShadow: '0 40px 80px rgba(0, 0, 0, 0.4), 0 0 0 1px rgba(20, 184, 166, 0.1)',
          padding: '60px',
          backdropFilter: 'blur(20px)',
          border: '1px solid rgba(51, 65, 85, 0.8)',
          transform: isVisible ? 'translateY(0)' : 'translateY(20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.4s',
          width: '100%'
        }}>
          {/* Card Header */}
          <div style={{ textAlign: 'center', marginBottom: '48px' }}>
            <h2 style={{
              fontSize: '36px',
              fontWeight: '700',
              background: 'linear-gradient(135deg, #14b8a6 0%, #0891b2 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
              marginBottom: '16px',
              margin: '0 0 16px 0'
            }}>
              Welcome Back
            </h2>
            <p style={{ 
              color: '#94a3b8', 
              fontSize: '18px',
              margin: '0',
              fontWeight: '500'
            }}>
              Sign in to access your clinic dashboard
            </p>
          </div>

          {/* Form */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} style={{ marginBottom: '32px' }}>
              <div style={{ marginBottom: '28px' }}>
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '15px',
                        fontWeight: '600',
                        color: '#e2e8f0',
                        marginBottom: '10px'
                      }}>
                        Email Address
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '18px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1,
                            color: focusedField === 'email' ? '#14b8a6' : '#64748b',
                            transition: 'color 0.3s ease'
                          }}>
                            <Mail size={22} />
                          </div>
                          <Input
                            type="email"
                            autoComplete="email"
                            placeholder="doctor@healthcarepro.com"
                            style={{
                              width: '100%',
                              paddingLeft: '54px',
                              paddingRight: '18px',
                              paddingTop: '16px',
                              paddingBottom: '16px',
                              border: `2px solid ${focusedField === 'email' ? '#14b8a6' : '#475569'}`,
                              borderRadius: '16px',
                              fontSize: '16px',
                              outline: 'none',
                              boxSizing: 'border-box',
                              transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                              boxShadow: focusedField === 'email' ? '0 0 0 4px rgba(20, 184, 166, 0.15), 0 0 20px rgba(20, 184, 166, 0.1)' : 'none',
                              transform: focusedField === 'email' ? 'translateY(-2px)' : 'translateY(0)',
                              backgroundColor: 'rgba(51, 65, 85, 0.5)',
                              color: '#f1f5f9',
                              backdropFilter: 'blur(10px)'
                            }}
                            {...field}
                            onFocus={() => setFocusedField('email')}
                            onBlur={() => setFocusedField(null)}
                          />
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '13px', color: '#f87171', marginTop: '8px' }} />
                    </FormItem>
                  )}
                />
              </div>

              <div style={{ marginBottom: '36px' }}>
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel style={{
                        display: 'block',
                        fontSize: '15px',
                        fontWeight: '600',
                        color: '#e2e8f0',
                        marginBottom: '10px'
                      }}>
                        Password
                      </FormLabel>
                      <FormControl>
                        <div style={{ position: 'relative' }}>
                          <div style={{
                            position: 'absolute',
                            left: '18px',
                            top: '50%',
                            transform: 'translateY(-50%)',
                            pointerEvents: 'none',
                            zIndex: 1,
                            color: focusedField === 'password' ? '#14b8a6' : '#64748b',
                            transition: 'color 0.3s ease'
                          }}>
                            <Lock size={22} />
                          </div>
                          <Input
                            type={showPassword ? 'text' : 'password'}
                            autoComplete="current-password"
                            placeholder="Enter your secure password"
                            style={{
                              width: '100%',
                              paddingLeft: '54px',
                              paddingRight: '54px',
                              paddingTop: '16px',
                              paddingBottom: '16px',
                              border: `2px solid ${focusedField === 'password' ? '#14b8a6' : '#475569'}`,
                              borderRadius: '16px',
                              fontSize: '16px',
                              outline: 'none',
                              boxSizing: 'border-box',
                              transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                              boxShadow: focusedField === 'password' ? '0 0 0 4px rgba(20, 184, 166, 0.15), 0 0 20px rgba(20, 184, 166, 0.1)' : 'none',
                              transform: focusedField === 'password' ? 'translateY(-2px)' : 'translateY(0)',
                              backgroundColor: 'rgba(51, 65, 85, 0.5)',
                              color: '#f1f5f9',
                              backdropFilter: 'blur(10px)'
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
                              right: '18px',
                              top: '50%',
                              transform: 'translateY(-50%)',
                              background: 'none',
                              border: 'none',
                              cursor: 'pointer',
                              color: '#64748b',
                              padding: '6px',
                              borderRadius: '8px',
                              transition: 'all 0.2s ease'
                            }}
                            onMouseEnter={(e) => {
                              e.currentTarget.style.color = '#14b8a6';
                              e.currentTarget.style.backgroundColor = 'rgba(20, 184, 166, 0.1)';
                            }}
                            onMouseLeave={(e) => {
                              e.currentTarget.style.color = '#64748b';
                              e.currentTarget.style.backgroundColor = 'transparent';
                            }}
                          >
                            {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                          </button>
                        </div>
                      </FormControl>
                      <FormMessage style={{ fontSize: '13px', color: '#f87171', marginTop: '8px' }} />
                    </FormItem>
                  )}
                />
              </div>

              {/* Forgot Password Link */}
              <div style={{ textAlign: 'right', marginBottom: '32px' }}>
                <button
                  type="button"
                  style={{
                    background: 'none',
                    border: 'none',
                    color: '#14b8a6',
                    fontSize: '14px',
                    fontWeight: '500',
                    cursor: 'pointer',
                    textDecoration: 'none',
                    transition: 'all 0.2s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.color = '#0891b2';
                    e.currentTarget.style.textDecoration = 'underline';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.color = '#14b8a6';
                    e.currentTarget.style.textDecoration = 'none';
                  }}
                >
                  <HelpCircle size={16} style={{ marginRight: '6px', verticalAlign: 'middle' }} />
                  Forgot Password?
                </button>
              </div>

              <Button
                type="submit"
                disabled={isSubmitting}
                style={{
                  width: '100%',
                  background: isSubmitting ? '#475569' : 'linear-gradient(135deg, #14b8a6 0%, #0891b2 100%)',
                  color: 'white',
                  border: 'none',
                  borderRadius: '16px',
                  padding: '18px 24px',
                  fontSize: '17px',
                  fontWeight: '600',
                  cursor: isSubmitting ? 'not-allowed' : 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '10px',
                  boxShadow: isSubmitting ? 'none' : '0 12px 24px rgba(20, 184, 166, 0.3), 0 0 0 1px rgba(20, 184, 166, 0.2)',
                  transition: 'all 0.3s cubic-bezier(0.16, 1, 0.3, 1)',
                  transform: isSubmitting ? 'scale(0.98)' : 'scale(1)',
                  position: 'relative',
                  overflow: 'hidden'
                }}
                onMouseEnter={(e) => {
                  if (!isSubmitting) {
                    e.currentTarget.style.transform = 'translateY(-3px) scale(1.02)';
                    e.currentTarget.style.boxShadow = '0 16px 32px rgba(20, 184, 166, 0.4), 0 0 0 1px rgba(20, 184, 166, 0.3)';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!isSubmitting) {
                    e.currentTarget.style.transform = 'translateY(0) scale(1)';
                    e.currentTarget.style.boxShadow = '0 12px 24px rgba(20, 184, 166, 0.3), 0 0 0 1px rgba(20, 184, 166, 0.2)';
                  }
                }}
              >
                {isSubmitting ? (
                  <>
                    <Loader2 size={22} style={{ animation: 'spin 1s linear infinite' }} />
                    Signing in...
                  </>
                ) : (
                  <>
                    <Shield size={20} />
                    Sign In Securely
                  </>
                )}
              </Button>
            </form>
          </Form>

         
               
        </div>

        {/* Footer */}
        <div style={{ 
          textAlign: 'center', 
          marginTop: '40px',
          transform: isVisible ? 'translateY(0)' : 'translateY(20px)',
          opacity: isVisible ? 1 : 0,
          transition: 'all 0.8s cubic-bezier(0.16, 1, 0.3, 1) 0.6s'
        }}>
          <p style={{ 
            fontSize: '14px', 
            color: '#64748b', 
            margin: '0',
            fontWeight: '500'
          }}>
            🔒 Secure • 🏥 Trusted • 💼 Professional Healthcare Management
          </p>
        </div>
      </div>

      {/* Enhanced CSS for medical-themed animations */}
      <style>{`
        /* Reset any default margins/padding that might cause white space */
        html, body {
          margin: 0 !important;
          padding: 0 !important;
          overflow-x: hidden;
        }
        
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
        
        @keyframes medicalFloat {
          0%, 100% { transform: translateY(0px) rotate(0deg); }
          33% { transform: translateY(-15px) rotate(2deg); }
          66% { transform: translateY(8px) rotate(-1deg); }
        }
        
        @keyframes medicalPulse {
          0%, 100% { 
            transform: scale(1);
            box-shadow: 0 25px 50px rgba(20, 184, 166, 0.3), 0 0 0 15px rgba(20, 184, 166, 0.1);
          }
          50% { 
            transform: scale(1.08);
            box-shadow: 0 30px 60px rgba(20, 184, 166, 0.4), 0 0 0 20px rgba(20, 184, 166, 0.15);
          }
        }
        
        @keyframes medicalSparkle {
          0%, 100% { 
            transform: scale(1) rotate(0deg);
            opacity: 1;
          }
          50% { 
            transform: scale(1.3) rotate(180deg);
            opacity: 0.7;
          }
        }
        
        @media (max-width: 640px) {
          .login-container {
            padding: 16px !important;
          }
          .login-card {
            padding: 40px 32px !important;
          }
        }
        
        @media (max-width: 480px) {
          .login-container {
            padding: 12px !important;
          }
          .login-card {
            padding: 32px 24px !important;
          }
        }
        
        /* Custom scrollbar for webkit browsers */
        ::-webkit-scrollbar {
          width: 8px;
        }
        
        ::-webkit-scrollbar-track {
          background: rgba(51, 65, 85, 0.3);
        }
        
        ::-webkit-scrollbar-thumb {
          background: rgba(20, 184, 166, 0.5);
          border-radius: 4px;
        }
        
        ::-webkit-scrollbar-thumb:hover {
          background: rgba(20, 184, 166, 0.7);
        }
      `}</style>
    </div>
  );
}