import { useState } from 'react';
import { apiService } from '@/services/api';
import { toast } from 'sonner';

interface UseApiState {
  loading: boolean;
  error: string | null;
}

export function useApi() {
  const [state, setState] = useState<UseApiState>({
    loading: false,
    error: null,
  });

  const handleRequest = async <T>(
    request: () => Promise<T>,
    showSuccessToast = false,
    successMessage = 'Operation completed successfully'
  ): Promise<T | null> => {
    try {
      setState({ loading: true, error: null });
      
      const response = await request();
      
      if (showSuccessToast) {
        toast.success(successMessage);
      }
      setState({ loading: false, error: null });
      return response;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An unexpected error occurred';
      setState({ loading: false, error: errorMessage });
      toast.error(errorMessage);
      return null;
    }
  };

  const get = <T>(url: string) => {
    return handleRequest(() => apiService.get<T>(url));
  };

  const post = <T>(url: string, data?: any, showSuccessToast = false, successMessage?: string) => {
    return handleRequest(() => apiService.post<T>(url, data), showSuccessToast, successMessage);
  };

  const put = <T>(url: string, data?: any, showSuccessToast = false, successMessage?: string) => {
    return handleRequest(() => apiService.put<T>(url, data), showSuccessToast, successMessage);
  };

  const del = <T>(url: string, showSuccessToast = false, successMessage?: string) => {
    return handleRequest(() => apiService.delete<T>(url), showSuccessToast, successMessage);
  };

  return {
    ...state,
    get,
    post,
    put,
    delete: del,
  };
}