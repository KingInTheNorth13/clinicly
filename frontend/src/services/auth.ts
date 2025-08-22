import { apiService } from './api';
import type { LoginRequest, AuthResponse, User } from '@/types';

class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await apiService.post<any>('/auth/login', credentials);
      
      console.log('Login response:', response);
      console.log('Response keys:', Object.keys(response));
      console.log('Response type:', typeof response);
      
      const { accessToken, refreshToken, user } = response;
      
      console.log('Extracted values:', {
        AccessToken: accessToken,
        RefreshToken: refreshToken,
        User: user,
        AccessTokenType: typeof accessToken,
        UserType: typeof user
      });
      
      // Validate that we have valid data before storing
      if (!accessToken || !user) {
        console.error('Missing required data:', { 
          AccessToken: !!accessToken, 
          User: !!user,
          fullResponse: response 
        });
        throw new Error('Invalid response from server');
      }
      
      // Store tokens in localStorage
      localStorage.setItem('authToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken || '');
      localStorage.setItem('user', JSON.stringify(user));
      
      console.log('Stored user data:', JSON.stringify(user));
      
      // Return in the format expected by frontend
      return {
        token: accessToken,
        refreshToken: refreshToken,
        user: user
      };
    } catch (error: any) {
      console.error('Login error details:', error);
      
      // Handle axios errors
      if (error.response?.status === 401) {
        throw new Error('Invalid email or password');
      } else if (error.response?.status === 400) {
        throw new Error('Please provide valid email and password');
      } else if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      } else {
        throw new Error(error.message || 'Login failed');
      }
    }
  }

  async logout(): Promise<void> {
    try {
      // Call logout endpoint to invalidate tokens on server
      await apiService.post('/auth/logout');
    } catch (error) {
      // Continue with logout even if server call fails
      console.error('Logout error:', error);
    } finally {
      // Clear local storage
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
    }
  }

  async refreshToken(): Promise<string> {
    const refreshToken = localStorage.getItem('refreshToken');
    
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    try {
      const response = await apiService.post<any>('/auth/refresh', {
        RefreshToken: refreshToken,
      });

      const { AccessToken } = response;
      localStorage.setItem('authToken', AccessToken);
      return AccessToken;
    } catch (error: any) {
      // Handle axios errors
      if (error.response?.status === 401) {
        throw new Error('Refresh token expired');
      } else {
        throw new Error(error.message || 'Token refresh failed');
      }
    }
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    if (userStr && userStr !== 'undefined' && userStr !== 'null') {
      try {
        return JSON.parse(userStr);
      } catch (error) {
        console.error('Error parsing user data:', error);
        // Clear invalid user data
        localStorage.removeItem('user');
        return null;
      }
    }
    return null;
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const user = this.getCurrentUser();
    
    // Check if we have both token and valid user data
    if (!token || !user) {
      // Clear any invalid data
      if (!user) {
        localStorage.removeItem('user');
      }
      if (!token) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
      }
      return false;
    }
    
    return true;
  }
}

export const authService = new AuthService();