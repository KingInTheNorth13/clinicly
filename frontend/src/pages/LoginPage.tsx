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
      <div className="fixed inset-0 flex items-center justify-center bg-background overflow-hidden">
        {/* Medical-themed background pattern */}
        <div className="absolute w-full h-full bg-grid-pattern animate-medical-float"></div>

        <div className={`text-center z-10 transition-all duration-1000 ease-in-out ${isVisible ? 'translate-y-0 scale-100 opacity-100' : 'translate-y-5 scale-95 opacity-0'}`}>
          <div className="w-20 h-20 bg-gradient-to-br from-teal-500 to-cyan-600 rounded-full flex items-center justify-center mx-auto mb-6 shadow-lg animate-medical-pulse border-2 border-teal-500/30">
            <Stethoscope size={40} className="text-white" />
          </div>
          <div className="w-10 h-10 border-4 border-teal-500/30 border-t-teal-500 rounded-full mx-auto mb-4 animate-spin"></div>
          <p className="text-lg text-foreground font-medium">
            Loading your session...
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center p-5 bg-background overflow-hidden">
      {/* Medical-themed background elements */}
      <div className="absolute w-full h-full bg-grid-pattern animate-medical-float"></div>

      {/* Floating medical icons */}
      <div className="absolute top-[15%] left-[10%] text-primary/20 animate-medical-float animation-delay-none">
        <Heart size={24} />
      </div>
      <div className="absolute top-[70%] right-[15%] text-blue-500/20 animate-medical-float animation-delay-2000">
        <Calendar size={20} />
      </div>
      <div className="absolute bottom-[25%] left-[20%] text-emerald-500/20 animate-medical-float animation-delay-1000">
        <Shield size={28} />
      </div>

      <div className={`w-full max-w-lg mx-auto z-10 flex flex-col items-center transition-all duration-1000 ease-in-out ${isVisible ? 'translate-y-0 scale-100 opacity-100' : 'translate-y-8 scale-95 opacity-0'}`}>
        {/* Logo and branding */}
        <div className={`text-center mb-10 w-full transition-all duration-1000 ease-in-out delay-200 ${isVisible ? 'translate-y-0 opacity-100' : '-translate-y-5 opacity-0'}`}>
          <div className="relative w-24 h-24 bg-gradient-to-br from-teal-500 to-cyan-600 rounded-full flex items-center justify-center mx-auto mb-6 shadow-lg animate-medical-pulse border-4 border-primary/20">
            <Stethoscope size={45} className="text-white" />
            <div className="absolute -top-2 -right-2 w-7 h-7 bg-gradient-to-br from-emerald-500 to-teal-500 rounded-full flex items-center justify-center animate-medical-sparkle border-2 border-background">
              <Heart size={12} className="text-white" />
            </div>
          </div>
          <h1 className="text-5xl font-extrabold text-foreground mb-3 tracking-tight text-shadow-lg">
            Clinicly
          </h1>
          <p className="text-lg text-muted-foreground font-medium">
            Advanced Clinic Management System
          </p>
        </div>

        {/* Login Card */}
        <div className={`w-full bg-card/80 backdrop-blur-xl border border-border/50 rounded-[28px] shadow-lg p-10 sm:p-12 transition-all duration-1000 ease-in-out delay-400 ${isVisible ? 'translate-y-0 opacity-100' : 'translate-y-5 opacity-0'}`}>
          <div className="text-center mb-10">
            <h2 className="text-4xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-primary to-cyan-500 mb-4">
              Welcome Back
            </h2>
            <p className="text-muted-foreground text-lg">
              Sign in to access your clinic dashboard
            </p>
          </div>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-7">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="block text-sm font-semibold text-foreground mb-2">Email Address</FormLabel>
                    <FormControl>
                      <div className="relative">
                        <Mail size={22} className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors duration-300 ${focusedField === 'email' ? 'text-primary' : 'text-muted-foreground'}`} />
                        <Input
                          type="email"
                          autoComplete="email"
                          placeholder="doctor@healthcarepro.com"
                          className={`w-full pl-12 pr-4 py-6 text-base bg-input border-2 rounded-xl transition-all duration-300 focus:ring-2 focus:ring-primary ${focusedField === 'email' ? 'border-primary shadow-lg' : 'border-border'}`}
                          {...field}
                          onFocus={() => setFocusedField('email')}
                          onBlur={() => setFocusedField(null)}
                        />
                      </div>
                    </FormControl>
                    <FormMessage className="text-sm text-destructive" />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="block text-sm font-semibold text-foreground mb-2">Password</FormLabel>
                    <FormControl>
                      <div className="relative">
                        <Lock size={22} className={`absolute left-4 top-1/2 -translate-y-1/2 transition-colors duration-300 ${focusedField === 'password' ? 'text-primary' : 'text-muted-foreground'}`} />
                        <Input
                          type={showPassword ? 'text' : 'password'}
                          autoComplete="current-password"
                          placeholder="Enter your secure password"
                          className={`w-full pl-12 pr-12 py-6 text-base bg-input border-2 rounded-xl transition-all duration-300 focus:ring-2 focus:ring-primary ${focusedField === 'password' ? 'border-primary shadow-lg' : 'border-border'}`}
                          {...field}
                          onFocus={() => setFocusedField('password')}
                          onBlur={() => setFocusedField(null)}
                        />
                        <button
                          type="button"
                          onClick={() => setShowPassword(!showPassword)}
                          className="absolute right-4 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-primary transition-colors duration-200 p-1 rounded-md"
                        >
                          {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                        </button>
                      </div>
                    </FormControl>
                    <FormMessage className="text-sm text-destructive" />
                  </FormItem>
                )}
              />

              <div className="text-right">
                <button
                  type="button"
                  className="text-sm font-medium text-primary hover:text-cyan-500 transition-colors duration-200 inline-flex items-center gap-2"
                >
                  <HelpCircle size={16} />
                  Forgot Password?
                </button>
              </div>

              <Button
                type="submit"
                disabled={isSubmitting}
                className="w-full text-lg font-semibold py-7 rounded-xl bg-gradient-to-r from-primary to-cyan-600 hover:from-primary/90 hover:to-cyan-600/90 text-primary-foreground shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting ? (
                  <>
                    <Loader2 size={22} className="animate-spin mr-2" />
                    Signing in...
                  </>
                ) : (
                  <>
                    <Shield size={20} className="mr-2" />
                    Sign In Securely
                  </>
                )}
              </Button>
            </form>
          </Form>
        </div>

        {/* Footer */}
        <div className={`text-center mt-10 transition-all duration-1000 ease-in-out delay-600 ${isVisible ? 'translate-y-0 opacity-100' : 'translate-y-5 opacity-0'}`}>
          <p className="text-sm text-muted-foreground font-medium">
            🔒 Secure • 🏥 Trusted • 💼 Professional Healthcare Management
          </p>
        </div>
      </div>

      <style>{`
        .bg-grid-pattern {
          background-image: radial-gradient(circle at 25% 25%, rgb(var(--medical-primary) / 0.1) 0%, transparent 50%),
                          radial-gradient(circle at 75% 75%, rgb(var(--medical-secondary) / 0.1) 0%, transparent 50%),
                          radial-gradient(circle at 50% 50%, rgb(var(--medical-success) / 0.05) 0%, transparent 50%);
        }
        .text-shadow-lg {
          text-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
        }
        .animation-delay-none { animation-delay: 0s; }
        .animation-delay-1000 { animation-delay: 1s; }
        .animation-delay-2000 { animation-delay: 2s; }
      `}</style>
    </div>
  );
}