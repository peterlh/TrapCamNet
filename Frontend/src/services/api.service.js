import axios from 'axios';
import authService from './auth.service';
import { auth } from '@/firebase/config';

// Create axios instance with base URL from environment variable
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  }
});

// Add request interceptor to include JWT token in requests
apiClient.interceptors.request.use(
  async (config) => {
    // Try to get a fresh token if the user is authenticated
    try {
      const currentUser = auth.currentUser;
      if (currentUser) {
        // Get a fresh token
        const token = await currentUser.getIdToken(false);
        localStorage.setItem('auth_token', token);
        config.headers['Authorization'] = `Bearer ${token}`;
      } else {
        // Fall back to stored token if no current user
        const token = authService.getAuthToken();
        if (token) {
          config.headers['Authorization'] = `Bearer ${token}`;
        }
      }
    } catch (err) {
      console.error('Error getting token for request:', err);
      // Still try to use the stored token
      const token = authService.getAuthToken();
      if (token) {
        config.headers['Authorization'] = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle token expiration
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    // Handle case where response might not exist
    if (!error.response) {
      console.error('Network error:', error);
      return Promise.reject(error);
    }
    
    const originalRequest = error.config;
    
    // If error is 401 (Unauthorized) and we haven't tried to refresh the token yet
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Get current user and refresh token
        const user = auth.currentUser;
        if (user) {
          const newToken = await user.getIdToken(true);
          localStorage.setItem('auth_token', newToken);
          
          // Update the authorization header
          originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
          return apiClient(originalRequest);
        } else {
          // If no user is logged in, redirect to login page
          authService.logout();
          window.location.href = '/#/pages/login';
          return Promise.reject(new Error('Authentication required'));
        }
      } catch (refreshError) {
        console.error('Token refresh error:', refreshError);
        // Redirect to login page if token refresh fails
        authService.logout();
        window.location.href = '/#/pages/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

// API service methods
const apiService = {
  // Get all images
  async getImages() {
    try {
      const response = await apiClient.get('/images');
      return response.data;
    } catch (error) {
      console.error('Error fetching images:', error);
      throw error;
    }
  },
  
  // Generic GET method
  async get(endpoint) {
    try {
      const response = await apiClient.get(endpoint);
      return response.data;
    } catch (error) {
      console.error(`Error fetching from ${endpoint}:`, error);
      throw error;
    }
  },
  
  // Generic POST method
  async post(endpoint, data) {
    try {
      const response = await apiClient.post(endpoint, data);
      return response.data;
    } catch (error) {
      console.error(`Error posting to ${endpoint}:`, error);
      throw error;
    }
  }
};

export default apiService;
